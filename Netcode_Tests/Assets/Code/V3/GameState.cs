using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NT3 {

	public struct GSI_Pos {
		public int m_id;
		public float m_x;
		public float m_y;
		public float m_z;
	}
	public struct GSI_Health {
		public int m_id;
		public float m_health;
	}
	public struct GSI_Arg {
		public int m_id;
		public byte m_arg;
	}

	public class GameState {

		public int m_refTick = -1;
		public List<GSI_Pos> m_positions = new List<GSI_Pos>();
		public List<GSI_Health> m_healths = new List<GSI_Health>();
		public List<GSI_Arg> m_arguments = new List<GSI_Arg>();

		bool m_isLerped;
		bool m_isDelta;
		public byte[] Encrypt() {
			return null;
		}

		public void Decrypt(byte[] msg, int offset) {

		}

		public bool CreateDelta(GameState reference) {
			return false;
		}

		public bool DismantleDelta(GameState reference) {
			return false;
		}

		public bool Lerp(GameState start, GameState end, int t) {
			return false;
		}
	}
}