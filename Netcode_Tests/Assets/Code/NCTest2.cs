using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class NCTest2 : MonoBehaviour {

#if UNITY_SERVER

	UdpClient udpServer;
	IPEndPoint remoteEP;

	int m_remoteMax = -1;

	void Start() {
		udpServer = new UdpClient(11000);
		remoteEP = new IPEndPoint(IPAddress.Any, 11000);
		Debug.Log("[Server] server is ready and lisents to" + remoteEP);
		udpServer.DontFragment = true;
	}

	private void OnDestroy() {
		udpServer.Dispose();
		udpServer.Close();
	}
	
	void Update() {
		if (udpServer.Available <= 0)
			return;

		byte[] data = udpServer.Receive(ref remoteEP); // listen on port 11000

		int[] values = new int[data.Length / sizeof(int)];
		Buffer.BlockCopy(data, 0, values, 0, data.Length);

		m_remoteMax = Mathf.Max(m_remoteMax, Mathf.Max(values));

		udpServer.Send(BitConverter.GetBytes(m_remoteMax), sizeof(int), remoteEP); // reply back
	}

#else

	UdpClient client;
	IPEndPoint ep;


	public string m_IP = "127.0.0.1";
	public int m_confirmedTick = -1;
	public int m_currentTick = 0;
	public int queueSize = 0;
	public Queue<int> ticks = new Queue<int>();

	void Start() {
		client = new UdpClient();
		ep = new IPEndPoint(IPAddress.Parse(m_IP), 11000); // endpoint where server is listening
		client.Connect(ep);
		client.DontFragment = true;
	}

	private void OnDestroy() {
		client.Dispose();
		client.Close();
	}

	void Update() {
		queueSize = ticks.Count;

		if (client.Available <= 0)
			return;

		byte[] receivedData = client.Receive(ref ep);
		int value = BitConverter.ToInt32(receivedData, 0);

		Debug.Log("receive " + value + " from " + ep.ToString());

		m_confirmedTick = value;
		while (ticks.Count > 0 && ticks.Peek() <= value) {
			ticks.Dequeue();
		}
	}


	void FixedUpdate() {
		ticks.Enqueue(m_currentTick);
		m_currentTick++;

		byte[] msg = new byte[ticks.Count * sizeof(int)];
		Buffer.BlockCopy(ticks.ToArray(), 0, msg, 0, ticks.Count * sizeof(int));

		client.Send(msg, msg.Length);

	}

#endif
}
