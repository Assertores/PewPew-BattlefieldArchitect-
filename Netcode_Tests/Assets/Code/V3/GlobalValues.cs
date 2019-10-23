﻿using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace NT3 {

	public class Client {
		public int m_ID;
		public bool m_isConnected;
		public IPEndPoint m_eP;
		public InputBuffer m_inputBuffer = new InputBuffer();//highend = maxTick;
		public RingBuffer<GameState> m_gameStates = new RingBuffer<GameState>();//lowend = confirmedTick;
	}
	public class GlobalValues : Singelton<GlobalValues> {

		public bool m_autoGenerated = true;

		public GameObject[] p_objectTypes = null;

		public int m_lockStepBufferSize = 6;

		public List<Client> m_clients = new List<Client>();

		public int m_mapWidth = 5;//TODO: this information hast to come from Rene
		public int m_mapHight = 5;//TODO: this information hast to come from Rene

		private void Start() {
#if !UNITY_SERVER
			m_clients.Add(new Client());
#endif

			if (m_autoGenerated) {
				Debug.LogError("Global Values where automaticly generated");
				return;
			}
		}
	}
}