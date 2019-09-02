using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using UnityEngine;

//Packed C -> S: int ID, int Tick, int SizesSize, int[SizesSize] Size, Input[SizesSize][Size[i]]
//Packed S -> C: int Tick, Gamestate[] States

enum InputType : byte {FORWARD, BACKWARD, LEFT, RIGHT, UP, DOWN }

[StructLayout(LayoutKind.Explicit)]
struct InputMessageItem {
	[FieldOffset(0)]
	public uint tick;
	[FieldOffset(sizeof(uint))]
	public InputType type;

	[FieldOffset(0)]
	[MarshalAs(UnmanagedType.ByValArray, SizeConst = sizeof(uint) + sizeof(InputType))]
	public byte[] data;
}

[StructLayout(LayoutKind.Explicit)]
struct GameStateMessageItem {
	[FieldOffset(0)]
	public uint tick;
	[FieldOffset(sizeof(uint))]
	public uint iD;
	[FieldOffset(2*sizeof(uint))]
	public Vector3 pos;

	[FieldOffset(0)]
	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2 * sizeof(uint) + 3 * sizeof(float))]
	public byte[] data;
}

public class LockstepTest : MonoBehaviour {

	public static Action<uint> DoTick;

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

	List<GameStateMessageItem> m_gameStates = new List<GameStateMessageItem>();//unordert
	List<InputMessageItem> m_inputs = new List<InputMessageItem>();

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
		Listen();
	}

	private void FixedUpdate() {
		GameStateMessageItem[] tick = m_gameStates.FindAll(x => x.tick == m_currentTick).ToArray();

		if (tick.Length == m_gameStates.Count || tick.Length == 0) {
			print("Network Pause");
			return;
		}

		//set gamestate on live data
		DoTick.Invoke(m_currentTick);

		foreach(var it in tick) {
			m_gameStates.Remove(it);
		}
	}

	void Listen() {
		if (socket.Available <= 0)
			return;

		byte[] receivedData = socket.Receive(ref ep);

		if (receivedData.Length == sizeof(int)) {
			int tmp = BitConverter.ToInt32(receivedData, 0);
			if (tmp >= 0) {
				m_iD = tmp;
				return;
			}
		}

		for (int i = 0; i < receivedData.Length; i += m_sizeofGameStateMessageItem) {//no check for duplicates
			GameStateMessageItem element = new GameStateMessageItem();
			Buffer.BlockCopy(receivedData, i, element.data, 0, m_sizeofGameStateMessageItem);
			m_gameStates.Add(element);
		}
	}

#endif
}
