using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;

//#define UNITY_SERVER
namespace PPBA
{
	enum MessageType : byte { NON, CONNECT, DISCONNECT, RECONNECT, NEWID }

	public class GameNetcode : Singleton<GameNetcode>
	{
		public static bool s_ServerIsTimedOut = false;

		#region Variables
		[SerializeField] bool _startInScene = false;

		public string _iP = "127.0.0.1";
		public int _serverPort = 11000;
		[SerializeField] int _playerCount = 1;
		[SerializeField] int _myID = -1;

		UdpClient socket;
		IPEndPoint _ep;
		[SerializeField] int _maxPackageSize = 1470;
		[SerializeField] float _serverTimeOut = 5.0f;
		float _lastPackageTime = float.MaxValue;

		int _currentID = 0;
		#endregion
#region NetCode
#if UNITY_SERVER
		int _nextID = 0;
		void Start()
		{
			if(_startInScene)
				ServerStart(_serverPort, _playerCount);
		}
		private void OnDestroy()
		{
			ServerShutDown();
		}

		private void Update()
		{
			Profiler.BeginSample("[Server] Listen");
			bool b = Listen();
			Profiler.EndSample();
			if(!b)
				return;
			if(GlobalVariables.s_instance._clients.FindAll(x => x._isConnected == true).Count < _playerCount)
				return;

			Profiler.BeginSample("[Server] Simulate");
			int tick = TickHandler.s_instance.Simulate();
			Profiler.EndSample();
			if(tick < 0)
				return;

			Profiler.BeginSample("[Server] Send");
			Send(tick);
			Profiler.EndSample();
		}

		bool Listen()
		{
			if(null == socket || socket.Available <= 0)
				return false;

			while(socket.Available > 0)
			{
				IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, _serverPort);
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
#if DB_NC
						Debug.Log("[Server] package type was not handled: " + messageType);
#endif
						break;
				}
			}

			return true;
		}

		/// NON:        byte Type, int ID, int PackagesTick, byte BitFieldSize, byte[] ReceavedPackageBitField, int tick, {InputType[] inputs}[] tickInputs
		void HandleNON(byte[] data, IPEndPoint ep)
		{
			int RemoteID = BitConverter.ToInt32(data, 1);

			client client = GlobalVariables.s_instance._clients.Find(x => x._id == RemoteID);
			if(client == null)
				return;

			//resend missing packages
			byte[] field = new byte[data[1 + 2 * sizeof(int)]];

			Buffer.BlockCopy(data, 2 + 2 * sizeof(int), field, 0, field.Length);
			int fieldTick = BitConverter.ToInt32(data, 1 + sizeof(int));

			if(client._gameStates[fieldTick] != default && fieldTick != 0 && field.Length != 0 &&
				client._gameStates[fieldTick]._receivedMessages.ToArray().Length == field.Length)
			{
				client._gameStates[fieldTick]._receivedMessages.FromArray(field);

				SendGameStateToClient(fieldTick, client);
			}

			int offset = 2 + 2 * sizeof(int) + field.Length;

			int startTick = BitConverter.ToInt32(data, offset);
			offset += sizeof(int);

#if DB_IS
			Debug.Log((MessageType)data[0] + ", " + RemoteID + ", " + fieldTick + ", " + field.Length + ", x, " + startTick);
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			for(int i = 0; i < offset; i++)
			{
				sb.Append(data[i] + ", ");
			}
			Debug.Log(sb.ToString());
#endif

			int tick = startTick;
			for(; offset < data.Length; tick++)
			{
				InputState tmp = new InputState();
				offset = tmp.Decrypt(RemoteID, data, offset);

				client._inputStates[tick] = tmp;
			}

			client._gameStates.FreeUpTo(startTick - 1);

#if DB_NC
			Debug.Log("Client: " + RemoteID + " has send ticks: " + startTick + " to " + (tick - 1));
#endif
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
			List<byte[]> state = client._gameStates[tick].Encrypt(_maxPackageSize);//if gamestate exiets max udp package size
#if DB_NC
			if(tick % 20 == 0)
				print("DeltaTick: " + tick + "\n" + client._gameStates[tick].ToString());
#endif

			for(byte i = 0; i < state.Count; i++)
			{
				if(client._gameStates[tick]._receivedMessages[i, 0])
					continue;

				msg.Clear();

				msg.Add((byte)MessageType.NON);
				msg.Add(i);
				msg.Add((byte)state.Count);
				msg.AddRange(BitConverter.GetBytes(tick));
				msg.AddRange(BitConverter.GetBytes(client._gameStates[tick]._refTick));
				msg.AddRange(state[i]);

				socket.Send(msg.ToArray(), msg.Count, client._ep);
			}

			if(client._gameStates[tick]._isDelta)
				client._gameStates[tick].DismantleDelta(client._gameStates[client._gameStates.GetLowEnd()]);//creates exagtly the same gamestate the client will have
		}

		void DoServerRestart()
		{
			ServerShutDown();
			/*
			Debug.Log("[SERVER] restart");
			ObjectPool.ResetAllObjectPools();
			TickHandler.s_instance.DoReset();
			GlobalVariables.s_instance._clients = new List<client>();
			*/
		}
#else
		void Start()
		{
			if(_startInScene)
				ClientConnect(_iP, _serverPort);
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
#if DB_NC
				Debug.Log("Server Timed Out");
#endif
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

			while(socket.Available > 0)
			{
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
				element = new GameState(true);
				GlobalVariables.s_instance._clients[0]._gameStates[tick] = element;
			}

			//Debug.Log("decrypting tick " + tick);
			//Debug.Log("Decrypt package: " + data[1] + " of " + data[2]);
			element.Decrypt(data, 3 + 2 * sizeof(int), data[1], data[2]);
			element._refTick = BitConverter.ToInt32(data, 3 + sizeof(int));
			//Debug.Log("Decrypt: " + tick + " | ref: " + element._refTick);
#if DB_GS
			if(TickHandler.s_currentTick % 20 == 0)
				Debug.Log("DeltaTick: " + TickHandler.s_currentTick + "\n" + element.ToString());
#endif

			//if(!element._isEncrypted)
				GlobalVariables.s_instance._clients[0]._inputStates.FreeUpTo(tick + 1);
		}

		void HandleNewID(byte[] data)
		{
			_myID = BitConverter.ToInt32(data, 1);
			GlobalVariables.s_instance._clients[0]._id = _myID;
			GlobalVariables.s_instance._clients[0]._isConnected = true;
		}


		
		/// NON:        byte Type, int ID, int PackagesTick, byte BitFieldSize, byte[] ReceavedPackageBitField, int tick, {InputType[] inputs}[] tickInputs
		void Send()
		{
			List<byte> msg = new List<byte>();
			msg.Add((byte)MessageType.NON);
			msg.AddRange(BitConverter.GetBytes(GlobalVariables.s_instance._clients[0]._id));

			RingBuffer<InputState> ib = GlobalVariables.s_instance._clients[0]._inputStates;

			//----- obsolite -----
			//byte[] field = new byte[0];
			//if(GlobalVariables.s_instance._clients[0]._gameStates[ib.GetLowEnd()] != null)
			//	field = GlobalVariables.s_instance._clients[0]._gameStates[ib.GetLowEnd()]?._receivedMessages.ToArray();

			byte[] field = GlobalVariables.s_instance._clients[0]._gameStates[GlobalVariables.s_instance._clients[0]._gameStates.GetHighEnd()]._receivedMessages.ToArray();

			msg.AddRange(BitConverter.GetBytes(GlobalVariables.s_instance._clients[0]._gameStates.GetHighEnd()));
			msg.Add((byte)field.Length);
			msg.AddRange(field);

			msg.AddRange(BitConverter.GetBytes(ib.GetLowEnd()));
			for(int i = ib.GetLowEnd(); i < ib.GetHighEnd(); i++)
			{
				if(ib[i] == default)
				{
					msg.AddRange(InputState.s_emptyInputState);
					continue;
				}

				byte[] tmp = ib[i].Encrypt();
				if(tmp == null)
				{
					msg.AddRange(InputState.s_emptyInputState);
					continue;
				}

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
			StatusNetcode.s_instance.StartServer(port, playerCount);
			_serverPort = port;
			_playerCount = playerCount;
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

			SceneManager.LoadScene(StringCollection.MAINMENU);
		}
#endregion
	}
}
