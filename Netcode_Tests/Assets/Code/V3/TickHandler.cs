using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;

//#define UNITY_SERVER
namespace NT3 {
	public class TickHandler : Singelton<TickHandler> {

		int m_currentTick = 0;

		public static System.Action<int, InputBuffer[]> s_DoTick = null;

#if UNITY_SERVER
        class Client {//weil c# dum ist und mir eine kopie des structs zurück giebt anstadt eine reference auf das struct (Ligt an List bei array würde es funktionieren)
            
            public InputBuffer m_inputBuffer = new InputBuffer();//highend = maxTick;
            public RingBuffer<GameState> m_gameStates = new RingBuffer<GameState>();//lowend = confirmedTick;
        }

        List<Client> m_clients = new List<Client>();
#else

		RingBuffer<GameState> m_gameStates = new RingBuffer<GameState>();
		[SerializeField] InputHandler m_inputHandler;
		public InputBuffer m_input { get; private set; }
#endif
#if UNITY_SERVER
		public int Simulate() {
			int min = int.MaxValue;
			foreach(var it in m_clients) {
				if (it.m_isConnected && it.m_inputBuffer.GetHighEnd() < min)//Game goes on if some clients disconnect
					min = it.m_inputBuffer.GetHighEnd();
			}

			if (min == int.MaxValue)
				return m_currentTick;

			InputBuffer[] inputs = GetInputs();
			for(; m_currentTick < min; m_currentTick++) {
				s_DoTick?.Invoke(m_currentTick, inputs);
			}
			foreach(var it in m_clients) {
				it.m_gameStates[m_currentTick] = new GameState();
				//TODO: Gamestates zu den clients speichern
			}

			return m_currentTick;
		}

		public void AddInputToClient(int clientID,int firstTick, InputElement[] element) {
			if (!m_clients.Exists(x => x.m_inputBuffer.m_iD == clientID))
				return;

			InputBuffer inputs = m_clients.Find(x => x.m_inputBuffer.m_iD == clientID).m_inputBuffer;

			for(int i = inputs.GetHighEnd(); i < firstTick + element.Length; i++) {
				inputs.AddNewElement(element[i - firstTick], i);
			}
		}

		public void AddNewClient() {
			Client tmp = new Client();
			tmp.m_inputBuffer.m_iD = m_clients.Count;

			m_clients.Add(tmp);
		}
#else
		private void Awake() {
			if (!m_inputHandler) {
				Debug.Log("no InputHandler asigned");
				GameObject tmp = new GameObject("AUTO_InputHandler");
				m_inputHandler = tmp.AddComponent<InputHandler>();
			}

			m_input = m_inputHandler.m_inputBuffer;
		}
		private void FixedUpdate() {
			if(m_gameStates.GetHighEnd() < m_currentTick) {
				Debug.Log("Network Pause");
				return;
			}
			GameState nextState = default;
			int nextStateTick = m_currentTick;
			for(; nextState == default; nextStateTick++) {
				nextState = m_gameStates[nextStateTick];
			}
			if(nextStateTick != m_currentTick) {
				GameState tmp = new GameState();
				if(nextState.RefTick < m_gameStates.GetLowEnd() || m_gameStates[nextState.RefTick] == default) {
					Debug.LogError("Reference Tick for Lerp not Found");
					return;//no idea how to fix this
				}

				tmp.Lerp(m_gameStates[nextState.RefTick], nextState, m_currentTick);
				nextState = tmp;
			}

			//TODO: apply nextTick to live data

			s_DoTick?.Invoke(m_currentTick, GetInputs());
			m_currentTick++;
			m_input.CreateNewElement(m_currentTick);
		}

		public bool AddGameState(GameState newGameState, int tick) {
			if (tick < m_currentTick)
				return false;

			m_gameStates[tick] = newGameState;
			return true;
		}
#endif
		InputBuffer[] GetInputs() {
#if UNITY_SERVER
			List<InputBuffer> value = new List<InputBuffer>(m_clients.Count);
			foreach(var it in m_clients) {
				//if (it.m_isConnected)//only netcode knows who is connected
					value.Add(it.m_inputBuffer);
			}
			return value.ToArray();
#else
			return new InputBuffer[] { m_input };
#endif
		}
	}
}