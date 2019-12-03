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
		UdpClient socket;
		public int m_serverPort = 11000;

		IPEndPoint ep;
		public string m_iP = "127.0.0.1";

		public int m_maxPackageSize = 1470;

		int _currentID = 0;

#if UNITY_SERVER

		int _nextID = 0;
		[SerializeField] int _playerCount = 1;

		void Start()
		{
			socket = new UdpClient(11000);
			Debug.Log("[Server] server is ready and lisents");
			socket.DontFragment = true;

			TickHandler.s_GatherValues += AddNewIDsToGameState;
		}
		private void OnDestroy()
		{
			TickHandler.s_GatherValues -= AddNewIDsToGameState;

			if(null != socket)
				socket.Close();
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

			Debug.Log("[Server] reseaved package");

			IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 11000);
			byte[] data = socket.Receive(ref remoteEP);

			Debug.Log("[Server] retreaved package");

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

			Debug.Log("[Server] RemoteID: " + RemoteID);

			if(!GlobalVariables.s_instance._clients.Exists(x => x._id == RemoteID))
				return;

			client client = GlobalVariables.s_instance._clients.Find(x => x._id == RemoteID);
			if(client == null)
				return;

			Debug.Log("[Server] found Client");

			//resend missing packages
			byte[] field = new byte[data[1 + sizeof(int)]];

			Debug.Log("[Server] Bitfieldsize in byte: " + field.Length);

			Buffer.BlockCopy(data, 2 + sizeof(int), field, 0, field.Length);
			int fieldTick = BitConverter.ToInt32(data, 2 + sizeof(int) + field.Length);
			Debug.Log("[Server] fieldTick: " + fieldTick);

			if(client._gameStates[fieldTick] != default && fieldTick != 0 && field.Length != 0)
			{
				client._gameStates[fieldTick]._receivedMessages.FromArray(field);

				Debug.Log("[Server] resending tick : " + fieldTick + " to client: " + client._id);
				SendGameStateToClient(fieldTick, client);
			}

			int offset = 2 + sizeof(int) + field.Length;
			while(offset < data.Length)
			{
				int tick = BitConverter.ToInt32(data, offset);
				Debug.Log("[Server] retreaving input for tick: " + tick);

				offset += sizeof(int);

				InputState tmp = new InputState();
				offset = tmp.Decrypt(RemoteID, data, offset);

				Debug.Log("[Server] adding inputstate to client inputStateBuffer");
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

			Debug.Log("[Server] new client connected. id: " + (element._id));

			byte[] msg = new byte[1 + sizeof(int)];
			msg[0] = (byte)MessageType.NEWID;
			Buffer.BlockCopy(BitConverter.GetBytes(element._id), 0, msg, 1, sizeof(int));
			socket.Send(msg, msg.Length, ep);
		}

		void HandleDisconnect(byte[] data, IPEndPoint ep)
		{
			int RemoteID = BitConverter.ToInt32(data, 1);

			if(!GlobalVariables.s_instance._clients.Exists(x => x._id == RemoteID))
				return;

			client target = GlobalVariables.s_instance._clients.Find(x => x._id == RemoteID);
			target._isConnected = false;
			target._ep = ep;

			Debug.Log("[Server] client " + RemoteID + " disconnected");
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

			Debug.Log("[Server] client " + RemoteID + " reconnected");
		}

		/// NON:        byte Type, byte PackageNumber, byte PackageCount, int Tick, Gamestate[] states(with reftick)
		public void Send(int tick)
		{
			Debug.Log("[Server] sinding gamestates for tick: " + tick);
			foreach(var it in GlobalVariables.s_instance._clients)
			{
				SendGameStateToClient(tick, it);
			}
		}

		void SendGameStateToClient(int tick, client client)
		{
			Debug.Log("[Server] client: " + client._id + "is connected: " + client._isConnected);

			if(!client._isConnected)
				return;

			Debug.Log("[Server] creating delta to tick: " + client._gameStates.GetLowEnd());

			client._gameStates[tick].CreateDelta(client._gameStates, client._gameStates.GetLowEnd(), tick);

			List<byte> msg = new List<byte>();
			List<byte[]> state = client._gameStates[tick].Encrypt(m_maxPackageSize);//if gamestate exiets max udp package size
			for(byte i = 0; i < state.Count; i++)
			{
				Debug.Log("[Server] adding header");
				msg.Clear();

				msg.Add((byte)MessageType.NON);
				msg.Add(i);
				msg.Add((byte)state.Count);
				msg.AddRange(BitConverter.GetBytes(tick));
				msg.AddRange(state[i]);

				Debug.Log("[Server] sending message: " + i + "of: " + state.Count);
				socket.Send(msg.ToArray(), msg.Count, client._ep);
			}

			client._gameStates[tick].DismantleDelta(client._gameStates[client._gameStates.GetLowEnd()]);//creates exagtly the same gamestate the client will have
		}
#else

		[SerializeField] int _myID = -1;

		void Start()
		{
			socket = new UdpClient();
			ep = new IPEndPoint(IPAddress.Parse(m_iP), m_serverPort); // endpoint where server is listening
			socket.Connect(ep);
			socket.DontFragment = true;
			
			if(_myID < 0)
			{
				byte[] msg = new byte[1];
				msg[0] = (byte)MessageType.CONNECT;
				socket.Send(msg, msg.Length);
			}
			else
			{
				byte[] msg = new byte[1 + sizeof(int)];
				msg[0] = (byte)MessageType.RECONNECT;
				Buffer.BlockCopy(BitConverter.GetBytes(_myID),0,msg,1,sizeof(int));
				socket.Send(msg, msg.Length);
			}
		}
		private void OnDestroy()
		{
			byte[] msg = new byte[1 + sizeof(int)];
			msg[0] = (byte)MessageType.DISCONNECT;
			Buffer.BlockCopy(BitConverter.GetBytes(_myID), 0, msg, 1, sizeof(int));
			socket.Send(msg, msg.Length);

			socket.Close();
		}

		private void Update()
		{
			Listen();
		}

		private void FixedUpdate()
		{
			Send();
		}

		void Listen()
		{
			if(socket.Available <= 0)
				return;

			byte[] data = socket.Receive(ref ep);

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

		/// NON:        byte Type, byte PackageNumber, byte PackageCount, int Tick, Gamestate[] states(with reftick)
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

			element.Decrypt(data, 3 + sizeof(int), data[1], data[2]);

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

		//	print(ib.GetLowEnd() + " | " + ib.GetHighEnd());

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
		}
	}
}
