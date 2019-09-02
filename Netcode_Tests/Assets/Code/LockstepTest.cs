using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using UnityEngine;

enum InputType : byte {FORWARD, BACKWARD, LEFT, RIGHT, UP, DOWN }

[StructLayout(LayoutKind.Explicit, Size = sizeof(uint) + sizeof(InputType))]
struct InputMessageItem {
	[FieldOffset(0)]
	public uint tick;
	[FieldOffset(sizeof(uint))]
	public InputType type;

	[FieldOffset(0)]
	[MarshalAs(UnmanagedType.ByValArray, SizeConst = sizeof(uint) + sizeof(InputType))]
	public byte[] data;
}

[StructLayout(LayoutKind.Explicit, Size = 2 * sizeof(uint) + 3 * sizeof(float))]
struct GameStateMessageItem {
	[FieldOffset(0)]
	uint tick;
	[FieldOffset(sizeof(uint))]
	uint iD;
	[FieldOffset(2*sizeof(uint))]
	Vector3 pos;

	[FieldOffset(0)]
	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2 * sizeof(uint) + 3 * sizeof(float))]
	public byte[] data;
}

public class LockstepTest : MonoBehaviour {

	uint m_currentTick = 0;
	UdpClient socket;

	public int m_serverPort = 11000;

	int m_sizeofInputMessageItem = Marshal.SizeOf<InputMessageItem>();
	int m_sizeofGameStateMessageItem = Marshal.SizeOf<GameStateMessageItem>();

#if UNITY_SERVER

	void Start() {

	}
	
	void Update() {

	}

#else

	IPEndPoint ep;
	public string m_iP = "127.0.0.1";
	
	public uint m_confirmedTick = 0;

	public int m_iD = -1;

	void Start() {
		socket = new UdpClient();
		ep = new IPEndPoint(IPAddress.Parse(m_iP), m_serverPort); // endpoint where server is listening
		socket.Connect(ep);
		socket.DontFragment = true;

		socket.Send(BitConverter.GetBytes(m_iD), sizeof(int), ep);
	}

	private void OnDestroy() {
		socket.Close();
	}

	void Update() {
		if (socket.Available <= 0)
			return;

		byte[] receivedData = socket.Receive(ref ep);

		if(receivedData.Length == sizeof(int)) {
			m_iD = BitConverter.ToInt32(receivedData, 0);
			return;
		}

		GameStateMessageItem[] newGameState = new GameStateMessageItem[receivedData.Length / Marshal.SizeOf<GameStateMessageItem>()];

	}

#endif
}
