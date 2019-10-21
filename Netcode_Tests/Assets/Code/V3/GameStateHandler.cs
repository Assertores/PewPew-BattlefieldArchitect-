using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NT3 {
	public class GameStateHandler : Singelton<GameStateHandler> {

		RingBuffer<GameStateObject> m_liveStateElements = new RingBuffer<GameStateObject>();

		public GameState RetreveCurrentGameState() {
			GameState value = new GameState();

			for (int i = m_liveStateElements.GetLowEnd(); i <= m_liveStateElements.GetHighEnd(); i++) {
				GSI_Transform pos = new GSI_Transform() {
					m_id = i,
					m_x = m_liveStateElements[i].transform.position.x,
					m_y = m_liveStateElements[i].transform.position.y,
					m_z = m_liveStateElements[i].transform.position.z
				};
				value.m_transforms.Add(pos);

				GSI_Health health = new GSI_Health() {
					m_id = i,
					m_health = m_liveStateElements[i].m_health
				};
				value.m_healths.Add(health);

				GSI_Arg arg = new GSI_Arg() {
					m_id = i,
					m_arg = m_liveStateElements[i].m_arg
				};
				value.m_arguments.Add(arg);
			}

			return value;
		}

		public void SetGameState(GameState gamestate) {
			foreach (var it in gamestate.m_transforms) {
				m_liveStateElements[it.m_id].m_pos.x = it.m_x;
				m_liveStateElements[it.m_id].m_pos.y = it.m_y;
				m_liveStateElements[it.m_id].m_pos.z = it.m_z;
			}
			foreach (var it in gamestate.m_healths) {
				m_liveStateElements[it.m_id].m_health = it.m_health;
			}
			foreach (var it in gamestate.m_arguments) {
				m_liveStateElements[it.m_id].m_arg = it.m_arg;
			}
		}
	}
}