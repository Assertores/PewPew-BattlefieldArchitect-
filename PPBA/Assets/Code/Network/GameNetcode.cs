﻿using System;
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

#if UNITY_SERVER

		int _nextID = 0;

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

			TickHandler.s_instance.Simulate();
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

			if (!GlobalVariables.s_clients.Exists(x => x._id == RemoteID))
				return;

			client client = GlobalVariables.s_clients.Find(x => x._id == RemoteID);
			if (client == null)
				return;

			//resend missing packages
			byte[] field = new byte[data[1 + sizeof(int)]];
			Buffer.BlockCopy(data, 2 + sizeof(int), field, 0, field.Length);
			int fieldTick = BitConverter.ToInt32(data, 2 + sizeof(int) + field.Length);
			client._gameStates[fieldTick]._receivedMessages.FromArray(field);
			SendGameStateToClient(fieldTick, client);

			int offset = 2 + sizeof(int) + field.Length;
			while (offset < data.Length) {
				int tick = BitConverter.ToInt32(data, offset);
				offset += sizeof(int);

				InputState tmp = new InputState();
				offset = tmp.Decrypt(RemoteID, data, offset);
				client._inputStates[tick] = tmp;
			}
		}

		void HandleConnect(byte[] data, IPEndPoint ep) {
			client element = new client();

			element._isConnected = true;
			element._ep = ep;
			element._id = _nextID;
			_nextID++;

			GlobalVariables.s_clients.Add(element);

			Debug.Log("[Server] new client connected. id: " + (element._id));

			byte[] msg = new byte[1 + sizeof(int)];
			msg[0] = (byte)MessageType.NEWID;
			Buffer.BlockCopy(BitConverter.GetBytes(element._id), 0, msg, 1, sizeof(int));
			socket.Send(msg, msg.Length, ep);
		}

		void HandleDisconnect(byte[] data, IPEndPoint ep) {
			int RemoteID = BitConverter.ToInt32(data, 1);

			if (!GlobalVariables.s_clients.Exists(x => x._id == RemoteID))
				return;

			client target = GlobalVariables.s_clients.Find(x => x._id == RemoteID);
			target._isConnected = false;
			target._ep = ep;

			Debug.Log("[Server] client " + RemoteID + " disconnected");
		}

		void HandleReconnect(byte[] data, IPEndPoint ep) {
			int RemoteID = BitConverter.ToInt32(data, 1);

			if (!GlobalVariables.s_clients.Exists(x => x._id == RemoteID)) {
				HandleConnect(data, ep);
				return;
			}

			client target = GlobalVariables.s_clients.Find(x => x._id == RemoteID);
			target._isConnected = true;
			target._ep = ep;

			Debug.Log("[Server] client " + RemoteID + " reconnected");
		}

		/// NON:        byte Type, byte PackageNumber, byte PackageCount, int Tick, Gamestate[] states(with reftick)
		public void Send(int tick) {
			foreach (var it in GlobalVariables.s_clients) {
				SendGameStateToClient(tick, it);
			}
		}

		void SendGameStateToClient(int tick, client client) {
			if (!client._isConnected)
				return;

			client._gameStates[tick].CreateDelta(client._gameStates, client._gameStates.GetLowEnd(), tick);

			List<byte> msg = new List<byte>();
			List<byte[]> state = client._gameStates[tick].Encrypt(m_maxPackageSize);//if gamestate exiets max udp package size
			for (byte i = 0; i < state.Count; i++) {
				msg.Clear();

				msg.Add((byte)MessageType.NON);
				msg.Add(i);
				msg.Add((byte)state.Count);
				msg.AddRange(BitConverter.GetBytes(tick));
				msg.AddRange(state[i]);

				socket.Send(msg.ToArray(), msg.Count, client._ep);
			}

			client._gameStates[tick].DismantleDelta(client._gameStates[client._gameStates.GetLowEnd()]);//creates exagtly the same gamestate the client will have
		}
#else
		void Start()
		{
			socket = new UdpClient();
			ep = new IPEndPoint(IPAddress.Parse(m_iP), m_serverPort); // endpoint where server is listening
			socket.Connect(ep);
			socket.DontFragment = true;

			byte[] msg = new byte[1];
			msg[0] = (byte)MessageType.CONNECT;
			socket.Send(msg, msg.Length);
		}
		private void OnDestroy()
		{
			byte[] msg = new byte[1 + sizeof(int)];
			msg[0] = (byte)MessageType.DISCONNECT;
			Buffer.BlockCopy(BitConverter.GetBytes(GlobalVariables.s_clients[0]._id), 0, msg, 1, sizeof(int));
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

			GameState element = GlobalVariables.s_clients[0]._gameStates[tick];
			if(element == default) //create new Gamestate if not already existing
			{
				element = new GameState();
				GlobalVariables.s_clients[0]._gameStates[tick] = element;
			}

			element.Decrypt(data, 3 + sizeof(int), data[1], data[2]);

			GlobalVariables.s_clients[0]._inputStates.FreeUpTo(tick);
		}

		void HandleNewID(byte[] data)
		{
			GlobalVariables.s_clients[0]._id = BitConverter.ToInt32(data, 1);
		}

		/// NON:        byte Type, int ID, byte BitFieldSize, byte[] ReceavedPackageBitField, {int tick, InputType[] inputs}[] tickInputs
		void Send()
		{
			List<byte> msg = new List<byte>();
			msg.Add((byte)MessageType.NON);
			msg.AddRange(BitConverter.GetBytes(GlobalVariables.s_clients[0]._id));

			RingBuffer<InputState> ib = GlobalVariables.s_clients[0]._inputStates;

			byte[] field = GlobalVariables.s_clients[0]._gameStates[ib.GetLowEnd()]._receivedMessages.ToArray();
			msg.Add((byte)field.Length);
			msg.AddRange(field);

			for(int i = ib.GetLowEnd(); i < ib.GetHighEnd(); i++)
			{
				byte[] tmp = ib[i].Encrypt();
				if(tmp == null)
					continue;

				msg.AddRange(BitConverter.GetBytes(i));
				msg.AddRange(tmp);
			}

			socket.Send(msg.ToArray(), msg.Count);
		}
#endif
	}
}
