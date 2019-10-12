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

		public static System.Action<int> s_DoTick = null;

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
		private void FixedUpdate() {
			if(GameStateHandler.s_singelton.m_gameStates.GetHighEnd() < m_currentTick) {
				Debug.Log("Network Pause");
				return;
			}
			GameState nextState = default;
			int nextStateTick = m_currentTick;
			for(; nextState == default; nextStateTick++) {
				nextState = GameStateHandler.s_singelton.m_gameStates[nextStateTick];
			}
			if(nextStateTick != m_currentTick) {
				GameState tmp = new GameState();
				if(nextState.m_refTick < GameStateHandler.s_singelton.m_gameStates.GetLowEnd() || GameStateHandler.s_singelton.m_gameStates[nextState.m_refTick] == default) {
					Debug.LogError("Reference Tick for Lerp not Found");
					return;//no idea how to fix this
				}

				tmp.Lerp(GameStateHandler.s_singelton.m_gameStates[nextState.m_refTick], nextState, m_currentTick);
				nextState = tmp;
			}

			GameStateHandler.s_singelton.SetGameState(m_currentTick);

			s_DoTick?.Invoke(m_currentTick);
			m_currentTick++;
			GlobalValues.s_singelton.m_clients[0].m_inputBuffer.CreateNewElement(m_currentTick);
		}

		public bool AddGameState(GameState newGameState, int tick) {
			if (tick < m_currentTick)
				return false;

			GameStateHandler.s_singelton.m_gameStates[tick] = newGameState;
			return true;
		}
#endif
	}
}