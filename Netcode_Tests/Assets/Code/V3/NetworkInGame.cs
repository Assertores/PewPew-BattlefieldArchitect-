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

        int nextID = 0;

		IPEndPoint ep;
		public string m_iP = "127.0.0.1";

#if UNITY_SERVER
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
			return false;
		}

		void Send(int tick) {

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
			Buffer.BlockCopy(BitConverter.GetBytes(TickHandler.s_singelton.m_input.m_iD), 0, msg, 1, sizeof(int));
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

		/// NON:        byte Type, int Tick, int RefTick, Gamestate[] states
		void HandleNON(byte[] data) {
			GameState element = new GameState();
			int tick = BitConverter.ToInt32(data, 1);

			Debug.Log("[Client] server State message: \n" + element.ToString());

			if (TickHandler.s_singelton.AddGameState(element, tick)) {
				element.Decrypt(data, 1 + sizeof(int));
				TickHandler.s_singelton.m_input.m_inputBuffer.FreeUpTo(tick);
			}
		}

		void HandleNewID(byte[] data) {
			TickHandler.s_singelton.m_input.m_iD = BitConverter.ToInt32(data, 1);
		}

		/// NON:        byte Type, int ID, {uint tick, int size, InputType[] inputs}[] tickInputs
		void Send() {
			List<byte> msg = new List<byte>();
			msg.Add((byte)MessageType.NON);
			msg.AddRange(BitConverter.GetBytes(TickHandler.s_singelton.m_input.m_iD));

			InputBuffer ib = TickHandler.s_singelton.m_input.m_inputBuffer;
			for(int i = ib.GetLowEnd(); i < ib.GetHighEnd(); i++) {
				byte[] tmp = ib[i].Encrypt();
				if (tmp == null)
					continue;

				msg.AddRange(BitConverter.GetBytes(i));
				msg.AddRange(BitConverter.GetBytes(tmp.Length));
				msg.AddRange(tmp);
			}

			socket.Send(msg.ToArray(), msg.Count);
		}
#endif
	}
}