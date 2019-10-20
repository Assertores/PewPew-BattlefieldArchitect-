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
			foreach(var it in GlobalValues.s_singelton.m_clients) {
				if (it.m_isConnected && it.m_inputBuffer.GetHighEnd() < min)//Game goes on if some clients disconnect
					min = it.m_inputBuffer.GetHighEnd();
			}

			if (min == int.MaxValue)
				return m_currentTick;

			for(; m_currentTick < min; m_currentTick++) {
				s_DoTick?.Invoke(m_currentTick);
			}
			foreach(var it in GlobalValues.s_singelton.m_clients) {
				it.m_gameStates[m_currentTick] = GameStateHandler.s_singelton.RetreveCurrentGameState();
			}

			return m_currentTick;
		}
#else
		private void FixedUpdate() {
			if(GlobalValues.s_singelton.m_clients[0].m_gameStates.GetHighEnd() < m_currentTick) {
				Debug.Log("Network Pause");
				return;
			}
			GameState nextState = default;
			int nextStateTick = m_currentTick;
			for(; nextState == default; nextStateTick++) {
				nextState = GlobalValues.s_singelton.m_clients[0].m_gameStates[nextStateTick];
			}
			nextStateTick--;//nextStateTick++ will be executed once to often

			if(nextStateTick != m_currentTick) {
				GameState tmp = new GameState();
				if(nextState.m_refTick < GlobalValues.s_singelton.m_clients[0].m_gameStates.GetLowEnd() || GlobalValues.s_singelton.m_clients[0].m_gameStates[nextState.m_refTick] == default) {
					Debug.LogError("Reference Tick for Lerp not Found");
					return;//no idea how to fix this
				}

				tmp.Lerp(GlobalValues.s_singelton.m_clients[0].m_gameStates[nextState.m_refTick], nextState, m_currentTick);//TODO: Rework Lerp
				nextState = tmp;
			}

			GameStateHandler.s_singelton.SetGameState(nextState);

			s_DoTick?.Invoke(m_currentTick);
			m_currentTick++;
			GlobalValues.s_singelton.m_clients[0].m_inputBuffer.CreateNewElement(m_currentTick);
		}

		public bool AddGameState(GameState newGameState, int tick) {
			if (tick < m_currentTick)
				return false;

			GlobalValues.s_singelton.m_clients[0].m_gameStates[tick] = newGameState;
			return true;
		}
#endif
	}
}