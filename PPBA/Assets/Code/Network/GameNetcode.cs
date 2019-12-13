using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

//#define UNITY_SERVER
namespace PPBA
{
	enum MessageType : byte { NON, CONNECT, DISCONNECT, RECONNECT, NEWID }

	public class GameNetcode : Singleton<GameNetcode>
	{
		public static bool s_ServerIsTimedOut = false;

		#region Variables
		public string m_iP = "127.0.0.1";
		public int m_serverPort = 11000;
		[SerializeField] int _playerCount = 1;
		[SerializeField] int _myID = -1;

		UdpClient socket;
		IPEndPoint _ep;
		[SerializeField] int m_maxPackageSize = 1470;
		[SerializeField] float _serverTimeOut = 5.0f;
		float _lastPackageTime = float.MaxValue;

		int _currentID = 0;
		#endregion
		#region NetCode
#if UNITY_SERVER
		int _nextID = 0;
		void Start()
		{
			//ServerStart(m_serverPort, _playerCount);
		}
		private void OnDestroy()
		{
			ServerShutDown();
		}

		private void Update()
		{
			if(!Listen())
				return;
			if(GlobalVariables.s_instance._clients.FindAll(x => x._isConnected == true).Count < _playerCount)
				return;

			int tick = TickHandler.s_instance.Simulate();
			if(tick < 0)
				return;

			Send(tick);
		}

		bool Listen()
		{
			if(null == socket || socket.Available <= 0)
				return false;

			IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, m_serverPort);
			byte[] data = socket.Receive(ref remoteEP);

			MessageType messageType = (MessageType)data[0];
			switch(messageType)
			{
				case MessageType.NON:
					HandleNON(data, remoteEP);
					break;
				case MessageType.CONNECT:
					HandleConnect(data, remoteEP);
					break;
				case MessageType.DISCONNECT:
					HandleDisconnect(data, remoteEP);
					break;
				case MessageType.RECONNECT:
					HandleReconnect(data, remoteEP);
					break;
				default:
					Debug.Log("[Server] package type was not handled: " + messageType);
					break;
			}

			return true;
		}

		/// NON:        byte Type, int ID, byte BitFieldSize, byte[] ReceavedPackageBitField, {int tick, InputType[] inputs}[] tickInputs
		void HandleNON(byte[] data, IPEndPoint ep)
		{
			int RemoteID = BitConverter.ToInt32(data, 1);

			if(!GlobalVariables.s_instance._clients.Exists(x => x._id == RemoteID))
				return;

			client client = GlobalVariables.s_instance._clients.Find(x => x._id == RemoteID);
			if(client == null)
				return;

			//resend missing packages
			byte[] field = new byte[data[1 + sizeof(int)]];

			Buffer.BlockCopy(data, 2 + sizeof(int), field, 0, field.Length);
			int fieldTick = BitConverter.ToInt32(data, 2 + sizeof(int) + field.Length);

			if(client._gameStates[fieldTick] != default && fieldTick != 0 && field.Length != 0 &&
				client._gameStates[fieldTick]._receivedMessages.ToArray().Length == field.Length)
			{
				client._gameStates[fieldTick]._receivedMessages.FromArray(field);

				SendGameStateToClient(fieldTick, client);
			}

			int offset = 2 + sizeof(int) + field.Length;
			while(offset < data.Length)
			{
				int tick = BitConverter.ToInt32(data, offset);

				offset += sizeof(int);

				InputState tmp = new InputState();
				offset = tmp.Decrypt(RemoteID, data, offset);

				client._inputStates[tick] = tmp;
			}

			GlobalVariables.s_instance._clients.Find(x => x._id == RemoteID)._gameStates.FreeUpTo(fieldTick - 2);
		}

		void HandleConnect(byte[] data, IPEndPoint ep)
		{
			client element = new client();

			element._isConnected = true;
			element._ep = ep;
			element._id = _nextID;
			_nextID++;

			GlobalVariables.s_instance._clients.Add(element);

			byte[] msg = new byte[1 + sizeof(int)];
			msg[0] = (byte)MessageType.NEWID;
			Buffer.BlockCopy(BitConverter.GetBytes(element._id), 0, msg, 1, sizeof(int));
			socket.Send(msg, msg.Length, ep);

			Debug.Log("[SERVER] new client has connected and got assigned " + element._id);
		}

		void HandleDisconnect(byte[] data, IPEndPoint ep)
		{
			int RemoteID = BitConverter.ToInt32(data, 1);

			if(!GlobalVariables.s_instance._clients.Exists(x => x._id == RemoteID))
				return;

			client target = GlobalVariables.s_instance._clients.Find(x => x._id == RemoteID);
			target._isConnected = false;
			target._ep = ep;

			Debug.Log("[SERVER] client " + RemoteID + " has disconnected");

			bool allDisconnected = true;
			foreach(var it in GlobalVariables.s_instance._clients)
			{
				if(it._isConnected)
				{
					allDisconnected = false;
					break;
				}
			}
			if(allDisconnected)
			{
				DoServerRestart();
			}
		}

		void HandleReconnect(byte[] data, IPEndPoint ep)
		{
			int RemoteID = BitConverter.ToInt32(data, 1);

			if(!GlobalVariables.s_instance._clients.Exists(x => x._id == RemoteID))
			{
				HandleConnect(data, ep);
				return;
			}

			client target = GlobalVariables.s_instance._clients.Find(x => x._id == RemoteID);
			target._isConnected = true;
			target._ep = ep;

			Debug.Log("[SERVER] client " + RemoteID + " has reconnected");
		}

		/// NON:        byte Type, byte PackageNumber, byte PackageCount, int Tick, int RefTick, Gamestate[] states
		public void Send(int tick)
		{
			foreach(var it in GlobalVariables.s_instance._clients)
			{
				SendGameStateToClient(tick, it);
			}
		}

		void SendGameStateToClient(int tick, client client)
		{

			if(!client._isConnected)
				return;

			if(!client._gameStates[tick]._isEncrypted)
				client._gameStates[tick].CreateDelta(client._gameStates, client._gameStates.GetLowEnd(), tick);

			List<byte> msg = new List<byte>();
			List<byte[]> state = client._gameStates[tick].Encrypt(m_maxPackageSize);//if gamestate exiets max udp package size
			for(byte i = 0; i < state.Count; i++)
			{
				msg.Clear();

				msg.Add((byte)MessageType.NON);
				msg.Add(i);
				msg.Add((byte)state.Count);
				msg.AddRange(BitConverter.GetBytes(tick));
				msg.AddRange(BitConverter.GetBytes(client._gameStates[tick]._refTick));
				msg.AddRange(state[i]);

				socket.Send(msg.ToArray(), msg.Count, client._ep);
			}

			client._gameStates[tick].DismantleDelta(client._gameStates[client._gameStates.GetLowEnd()]);//creates exagtly the same gamestate the client will have
		}

		void DoServerRestart()
		{
			Debug.Log("[SERVER] restart");
			TickHandler.s_instance.DoReset();
			GlobalVariables.s_instance._clients = new List<client>();
		}
#else
		void Start()
		{
			//ClientConnect(m_iP, m_serverPort);
		}
		private void OnDestroy()
		{
			ClientDisconnect();
		}

		private void Update()
		{
			Listen();
		}

		private void FixedUpdate()
		{
			Send();
		}

		UIPopUpWindowRefHolder h_popUp;
		void Listen()
		{
			if(Time.unscaledTime - _lastPackageTime > _serverTimeOut)
			{
				s_ServerIsTimedOut = true;
				Debug.Log("Server Timed Out");
				if(null == h_popUp)
					h_popUp = UIPopUpWindowHandler.s_instance.CreateWindow("Server Timed Out");
			}

			if(socket.Available <= 0)
				return;

			if(null != h_popUp)
			{
				h_popUp.CloseWindow();
				h_popUp = null;
			}
			s_ServerIsTimedOut = false;
			_lastPackageTime = Time.unscaledTime;

			byte[] data = socket.Receive(ref _ep);

			MessageType messageType = (MessageType)data[0];

			switch(messageType)
			{
				case MessageType.NON:
					HandleNON(data);
					break;
				case MessageType.NEWID:
					HandleNewID(data);
					break;
				default:
					break;
			}
		}

		/// NON:        byte Type, byte PackageNumber, byte PackageCount, int Tick, int RefTick, Gamestate[] states
		void HandleNON(byte[] data)
		{
			int tick = BitConverter.ToInt32(data, 3);

			if(TickHandler.s_currentTick > tick) //no work for the past
				return;

			GameState element = GlobalVariables.s_instance._clients[0]._gameStates[tick];
			if(element == default) //create new Gamestate if not already existing
			{
				element = new GameState();
				GlobalVariables.s_instance._clients[0]._gameStates[tick] = element;
			}

			//Debug.Log("Decrypt package: " + data[1] + " of " + data[2]);
			element.Decrypt(data, 3 + 2 * sizeof(int), data[1], data[2]);
			element._refTick = BitConverter.ToInt32(data, 3 + sizeof(int));
			//Debug.Log("Decrypt: " + tick + " | ref: " + element._refTick);

			GlobalVariables.s_instance._clients[0]._inputStates.FreeUpTo(tick);
		}

		void HandleNewID(byte[] data)
		{
			GlobalVariables.s_instance._clients[0]._id = BitConverter.ToInt32(data, 1);
			_myID = BitConverter.ToInt32(data, 1);
		}

		/// NON:        byte Type, int ID, byte BitFieldSize, byte[] ReceavedPackageBitField, {int tick, InputType[] inputs}[] tickInputs
		void Send()
		{
			List<byte> msg = new List<byte>();
			msg.Add((byte)MessageType.NON);
			msg.AddRange(BitConverter.GetBytes(GlobalVariables.s_instance._clients[0]._id));

			RingBuffer<InputState> ib = GlobalVariables.s_instance._clients[0]._inputStates;

			byte[] field = new byte[0];
			if(GlobalVariables.s_instance._clients[0]._gameStates[ib.GetLowEnd()] != null)
				field = GlobalVariables.s_instance._clients[0]._gameStates[ib.GetLowEnd()]?._receivedMessages.ToArray();

			msg.Add((byte)field.Length);
			msg.AddRange(field);

			for(int i = ib.GetLowEnd(); i < ib.GetHighEnd(); i++)
			{
				if(ib[i] == default)
					continue;

				byte[] tmp = ib[i].Encrypt();
				if(tmp == null)
					continue;

				msg.AddRange(BitConverter.GetBytes(i));
				msg.AddRange(tmp);
			}

			socket.Send(msg.ToArray(), msg.Count);
		}
#endif

		private List<GSC.newIDRange> h_newIDs = new List<GSC.newIDRange>();
		/// <summary>
		/// reserves a range of consecutive ids
		/// </summary>
		/// <param name="range">the count of id you nead</param>
		/// <param name="type">the type of prefab object pool that should be tracked</param>
		/// <returns>the first id of the reserved id range </returns>
		public int GetNewIDRange(ObjectType type, int range = 1)
		{
			int id = _currentID;
			_currentID += range;

			h_newIDs.Add(new GSC.newIDRange { _id = id, _range = range, _type = type });

			return id;
		}

		void AddNewIDsToGameState(int tick)
		{
			TickHandler.s_interfaceGameState._newIDRanges.AddRange(h_newIDs);
			h_newIDs.Clear();
		}
		#endregion
		#region UIInterface
		public void ClientConnect(string ip, int port)
		{
			Debug.Log("[CLIENT] connecting to " + ip + " at port " + port);

			socket = new UdpClient();
			_ep = new IPEndPoint(IPAddress.Parse(ip), port); // endpoint where server is listening
			socket.Connect(_ep);
			socket.DontFragment = true;

			byte[] msg = new byte[1];
			msg[0] = (byte)MessageType.CONNECT;
			socket.Send(msg, msg.Length);
		}

		public void ClientDisconnect()
		{
			if(null == socket)
				return;

			Debug.Log("[CLIENT] disconnecting");

			byte[] msg = new byte[1 + sizeof(int)];
			msg[0] = (byte)MessageType.DISCONNECT;
			Buffer.BlockCopy(BitConverter.GetBytes(_myID), 0, msg, 1, sizeof(int));
			socket.Send(msg, msg.Length);

			socket.Close();
		}

		public void ServerStart(int port, int playerCount, int map = -1, int botLimit = -1, int hmRes = -1, bool regToMS = false)
		{
			m_serverPort = port;
			socket = new UdpClient(port);
			socket.DontFragment = true;

			TickHandler.s_GatherValues += AddNewIDsToGameState;
			Debug.Log("[SERVER] startup. listening on port " + port + ", and waiting for " + playerCount + " players");
		}

		public void ServerShutDown()
		{
			TickHandler.s_GatherValues -= AddNewIDsToGameState;

			if(null != socket)
				socket.Close();

			Debug.Log("[SERVER] shut down");
		}
		#endregion
	}
}
