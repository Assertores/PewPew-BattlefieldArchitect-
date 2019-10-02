using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NT3 {
	public class GameStateHandler : Singelton<GameStateHandler> {

		public RingBuffer<GameState> m_gameStates = new RingBuffer<GameState>();

		RingBuffer<GameStateObject> m_liveStateElements = new RingBuffer<GameStateObject>();

		public void RetreveGameState(int tick) {
			for (int i = m_liveStateElements.GetLowEnd(); i <= m_liveStateElements.GetHighEnd(); i++) {
				GSI_Pos pos = new GSI_Pos() {
					m_id = i,
					m_x = m_liveStateElements[i].transform.position.x,
					m_y = m_liveStateElements[i].transform.position.y,
					m_z = m_liveStateElements[i].transform.position.z
				};
				m_gameStates[tick].m_positions.Add(pos);

				GSI_Health health = new GSI_Health() {
					m_id = i,
					m_health = m_liveStateElements[i].m_health
				};
				m_gameStates[tick].m_healths.Add(health);

				GSI_Arg arg = new GSI_Arg() {
					m_id = i,
					m_arg = m_liveStateElements[i].m_arg
				};
				m_gameStates[tick].m_arguments.Add(arg);
			}
		}

		public void SetGameState(int tick) {
			foreach (var it in m_gameStates[tick].m_positions) {
				m_liveStateElements[it.m_id].m_pos.x = it.m_x;
				m_liveStateElements[it.m_id].m_pos.y = it.m_y;
				m_liveStateElements[it.m_id].m_pos.z = it.m_z;
			}
			foreach (var it in m_gameStates[tick].m_healths) {
				m_liveStateElements[it.m_id].m_health = it.m_health;
			}
			foreach (var it in m_gameStates[tick].m_arguments) {
				m_liveStateElements[it.m_id].m_arg = it.m_arg;
			}
		}
	}
}