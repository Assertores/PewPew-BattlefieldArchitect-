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

		public static System.Action<int> s_DoInput = null;
		public static System.Action<int> s_CalculateValues = null;
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
				s_DoInput?.Invoke(m_currentTick);
				s_CalculateValues?.Invoke(m_currentTick);
				s_DoTick?.Invoke(m_currentTick);
			}
			foreach(var it in GlobalValues.s_singelton.m_clients) {
				it.m_gameStates[m_currentTick] = GameStateHandler.s_singelton.RetreveCurrentGameState();
			}

			return m_currentTick;
		}
#else
		private void FixedUpdate() {
			Client me = GlobalValues.s_singelton.m_clients[0];
			if (me.m_gameStates.GetHighEnd() < m_currentTick || !me.m_gameStates[m_currentTick].m_receivedMessages.AreAllBytesActive()) {
				Debug.Log("Network Pause");
				return;
			}
			GameState nextState = default;
			int nextStateTick = m_currentTick;
			for(; nextState == default; nextStateTick++) {
				nextState = me.m_gameStates[nextStateTick];
			}
			nextStateTick--;//nextStateTick++ will be executed once to often

			if (nextState.m_refTick < me.m_gameStates.GetLowEnd() || me.m_gameStates[nextState.m_refTick] == default) {
				Debug.LogError("Reference Tick not Found");
				return;//no idea how to fix this
			}

			if (nextStateTick != m_currentTick) {
				GameState tmp = new GameState();

				tmp.Lerp(me.m_gameStates[nextState.m_refTick], nextState, m_currentTick);//TODO: Rework Lerp
				nextState = tmp;
			} else {
				nextState.DismantleDelta(me.m_gameStates[nextState.m_refTick], me.m_inputBuffer[nextStateTick].GetInputIDs());
			}

			s_DoInput?.Invoke(m_currentTick);
			s_CalculateValues?.Invoke(m_currentTick);
			s_DoTick?.Invoke(m_currentTick);
			m_currentTick++;

			me.m_inputBuffer.CreateNewElement(m_currentTick);
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