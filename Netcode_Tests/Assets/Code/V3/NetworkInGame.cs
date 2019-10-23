using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;


//#define UNITY_SERVER
namespace NT3 {

	enum MessageType : byte { NON, CONNECT, DISCONNECT, RECONNECT, NEWID }

	public class NetworkInGame : Singelton<NetworkInGame> {

		UdpClient socket;
		public int m_serverPort = 11000;

		IPEndPoint ep;
		public string m_iP = "127.0.0.1";

		public int m_maxPackageSize = 65500;

#if UNITY_SERVER

		int m_nextID = 0;

		void Start() {
			socket = new UdpClient(11000);
			Debug.Log("[Server] server is ready and lisents");
			socket.DontFragment = true;
		}
		private void OnDestroy() {
			socket.Close();
		}

		private void Update() {
			if (!Listen())
				return;

			int tick = TickHandler.s_singelton.Simulate();
			if (tick == int.MinValue)
				return;

			Send(tick);
		}

		bool Listen() {
			if (socket.Available <= 0)
				return false;

			IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 11000);
			byte[] data = socket.Receive(ref remoteEP);

			Debug.Log("[Server] reseaved package");

			MessageType messageType = (MessageType)data[0];
			switch (messageType) {
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
					Debug.Log("[Server] package type was not handled" + messageType);
					break;
			}

			return true;
		}

		/// NON:        byte Type, int ID, byte BitFieldSize, byte[] ReceavedPackageBitField, {int tick, InputType[] inputs}[] tickInputs
		void HandleNON(byte[] data, IPEndPoint ep) {
			int RemoteID = BitConverter.ToInt32(data, 1);

			if (!GlobalValues.s_singelton.m_clients.Exists(x => x.m_ID == RemoteID))
				return;

			Client client = GlobalValues.s_singelton.m_clients.Find(x => x.m_ID == RemoteID);
			if (client == null)
				return;

			//resend missing packages
			byte[] field = new byte[data[1 + sizeof(int)]];
			Buffer.BlockCopy(data, 2 + sizeof(int), field, 0, field.Length);
			int fieldTick = BitConverter.ToInt32(data, 2 + sizeof(int) + field.Length);
			client.m_gameStates[fieldTick].m_receivedMessages.FromArray(field);
			SendGameStateToClient(fieldTick, client);

			int offset = 2 + sizeof(int) + field.Length;
			while (offset < data.Length) {
				int tick = BitConverter.ToInt32(data, offset);
				offset += sizeof(int);

				InputElement tmp = new InputElement();
				offset = tmp.Decrypt(data, offset);
				client.m_inputBuffer.AddNewElement(tmp, tick);
			}
		}

		void HandleConnect(byte[] data, IPEndPoint ep) {
			Client element = new Client();

			element.m_isConnected = true;
			element.m_eP = ep;
			element.m_ID = m_nextID;
			m_nextID++;

			GlobalValues.s_singelton.m_clients.Add(element);

			Debug.Log("[Server] new client connected. id: " + (element.m_ID));

			byte[] msg = new byte[1 + sizeof(int)];
			msg[0] = (byte)MessageType.NEWID;
			Buffer.BlockCopy(BitConverter.GetBytes(element.m_ID), 0, msg, 1, sizeof(int));
			socket.Send(msg, msg.Length, ep);
		}

		void HandleDisconnect(byte[] data, IPEndPoint ep) {
			int RemoteID = BitConverter.ToInt32(data, 1);

			if (!GlobalValues.s_singelton.m_clients.Exists(x => x.m_ID == RemoteID))
				return;

			Client target = GlobalValues.s_singelton.m_clients.Find(x => x.m_ID == RemoteID);
			target.m_isConnected = false;
			target.m_eP = ep;

			Debug.Log("[Server] client " + RemoteID + " disconnected");
		}

		void HandleReconnect(byte[] data, IPEndPoint ep) {
			int RemoteID = BitConverter.ToInt32(data, 1);

			if (!GlobalValues.s_singelton.m_clients.Exists(x => x.m_ID == RemoteID)) {
				HandleConnect(data, ep);
				return;
			}

			Client target = GlobalValues.s_singelton.m_clients.Find(x => x.m_ID == RemoteID);
			target.m_isConnected = true;
			target.m_eP = ep;

			Debug.Log("[Server] client " + RemoteID + " reconnected");
		}

		/// NON:        byte Type, byte PackageNumber, byte PackageCount, int Tick, Gamestate[] states(with reftick)
		void Send(int tick) {
			foreach (var it in GlobalValues.s_singelton.m_clients) {
				SendGameStateToClient(tick, it);
			}
		}

		void SendGameStateToClient(int tick, Client client) {
			if (!client.m_isConnected)
				return;

			client.m_gameStates[tick].CreateDelta(client.m_gameStates, client.m_gameStates.GetLowEnd(), tick - client.m_gameStates.GetLowEnd());

			List<byte> msg = new List<byte>();
			List<byte[]> state = client.m_gameStates[tick].Encrypt(m_maxPackageSize);//if gamestate exiets max udp package size
			for (byte i = 0; i < state.Count; i++) {
				msg.Clear();

				msg.Add((byte)MessageType.NON);
				msg.Add(i);
				msg.Add((byte)state.Count);
				msg.AddRange(BitConverter.GetBytes(tick));
				msg.AddRange(state[i]);

				socket.Send(msg.ToArray(), msg.Count, client.m_eP);
			}

			client.m_gameStates[tick].DismantleDelta(client.m_gameStates[client.m_gameStates.GetLowEnd()]);//creates exagtly the same gamestate the client will have
		}
#else
		void Start() {
			socket = new UdpClient();
			ep = new IPEndPoint(IPAddress.Parse(m_iP), m_serverPort); // endpoint where server is listening
			socket.Connect(ep);
			socket.DontFragment = true;

			byte[] msg = new byte[1];
			msg[0] = (byte)MessageType.CONNECT;
			socket.Send(msg, msg.Length);
		}
		private void OnDestroy() {
			byte[] msg = new byte[1 + sizeof(int)];
			msg[0] = (byte)MessageType.DISCONNECT;
			Buffer.BlockCopy(BitConverter.GetBytes(GlobalValues.s_singelton.m_clients[0].m_ID), 0, msg, 1, sizeof(int));
			socket.Send(msg, msg.Length);

			socket.Close();
		}

		private void Update() {
			Listen();
		}

		private void FixedUpdate() {
			Send();
		}

		void Listen() {
			if (socket.Available <= 0)
				return;

			byte[] data = socket.Receive(ref ep);

			MessageType messageType = (MessageType)data[0];

			switch (messageType) {
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
		void HandleNON(byte[] data) {
			int tick = BitConverter.ToInt32(data, 3);
			

			GameState element = GlobalValues.s_singelton.m_clients[0].m_gameStates[tick];

			if (element == default) {
				element = new GameState();
				TickHandler.s_singelton.AddGameState(element, tick);
				element.m_messageCount = data[2];
				element.m_receivedMessages = new BitField2D(data[2], 1);
			}

			Debug.Log("[Client] server State message: \n" + element.ToString());

			if (!element.m_receivedMessages[data[1], 0]) {
				element.Decrypt(data, 3 + sizeof(int));
				element.m_receivedMessages[data[1], 0] = true;
			}

			GlobalValues.s_singelton.m_clients[0].m_inputBuffer.FreeUpTo(tick);
		}

		void HandleNewID(byte[] data) {
			GlobalValues.s_singelton.m_clients[0].m_ID = BitConverter.ToInt32(data, 1);
		}

		/// NON:        byte Type, int ID, byte BitFieldSize, byte[] ReceavedPackageBitField, {int tick, InputType[] inputs}[] tickInputs
		void Send() {
			List<byte> msg = new List<byte>();
			msg.Add((byte)MessageType.NON);
			msg.AddRange(BitConverter.GetBytes(GlobalValues.s_singelton.m_clients[0].m_ID));

			InputBuffer ib = GlobalValues.s_singelton.m_clients[0].m_inputBuffer;

			byte[] field = GlobalValues.s_singelton.m_clients[0].m_gameStates[ib.GetLowEnd()].m_receivedMessages.ToArray();
			msg.Add((byte)field.Length);
			msg.AddRange(field);

			for(int i = ib.GetLowEnd(); i < ib.GetHighEnd(); i++) {
				byte[] tmp = ib[i].Encrypt();
				if (tmp == null)
					continue;

				msg.AddRange(BitConverter.GetBytes(i));
				msg.AddRange(tmp);
			}

			socket.Send(msg.ToArray(), msg.Count);
		}
#endif
	}
}