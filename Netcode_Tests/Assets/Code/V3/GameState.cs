using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NT3 {

	public struct GSI_Type{
		public int m_id;
		public int m_type;
	}

	public struct GSI_Transform {
		public int m_id;
		public float m_x;
		public float m_y;
		public float m_z;
		public float m_alpha;
	}

	public struct GSI_Scale {
		public int m_id;
		public float m_length;
	}

	public struct GSI_Ammo {
		public int m_id;
		public int m_bullets;
		public int m_grenades;
	}

	public struct GSI_Paths {
		public int m_id;
		public List<Vector3> m_path;
	}

	public struct GSI_Health {
		public int m_id;
		public float m_health;
	}
	public struct GSI_Arg {
		public int m_id;
		public byte m_arg;
	}

	public struct GSI_Behaviour {
		public int m_id;
		public byte m_behaviour;
	}

	public struct GSI_Map {
		public int m_id;
		public BitField2D m_mask;
		public List<float> m_values;
	}

	public class GameState {

		public int m_refTick = -1;
		public byte m_messageCount;
		public BitField2D m_receivedMessages;

		public List<GSI_Type> m_types = new List<GSI_Type>();
		public List<GSI_Transform> m_transforms = new List<GSI_Transform>();
		public List<GSI_Scale> m_scales = new List<GSI_Scale>();
		public List<GSI_Ammo> m_ammos = new List<GSI_Ammo>();
		public List<GSI_Paths> m_paths = new List<GSI_Paths>();
		public List<GSI_Health> m_healths = new List<GSI_Health>();
		public List<GSI_Arg> m_arguments = new List<GSI_Arg>();
		public List<GSI_Behaviour> m_behaviours = new List<GSI_Behaviour>();
		public List<GSI_Map> m_maps = new List<GSI_Map>();

		bool m_isLerped;
		bool m_isDelta;
		public List<byte[]> Encrypt(int maxPackageSize) {
			return new List<byte[]>();
		}

		public void Decrypt(byte[] msg, int offset) {

		}

		public bool CreateDelta(RingBuffer<GameState> reference, int tick) {
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