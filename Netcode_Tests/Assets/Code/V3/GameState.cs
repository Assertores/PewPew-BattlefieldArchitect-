using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NT3 {

	namespace GSI {
		public struct type {
			public int m_id;
			public int m_type;
		}

		public struct transform {
			public int m_id;
			public float m_x;
			public float m_y;
			public float m_z;
			public float m_alpha;
		}

		public struct scale {
			public int m_id;
			public float m_length;
		}

		public struct ammo {
			public int m_id;
			public int m_bullets;
			public int m_grenades;
		}

		public struct paths {
			public int m_id;
			public List<Vector3> m_path;
		}

		public struct health {
			public int m_id;
			public float m_health;
		}
		public struct arg {
			public int m_id;
			public byte m_arg;
		}

		public struct behaviour {
			public int m_id;
			public byte m_behaviour;
		}

		public struct map {
			public int m_id;
			public BitField2D m_mask;
			public List<float> m_values;
		}
	}

	public class GameState {

		public int m_refTick = -1;
		public byte m_messageCount;
		public BitField2D m_receivedMessages;

		public List<GSI.type> m_types = new List<GSI.type>();
		public List<GSI.transform> m_transforms = new List<GSI.transform>();
		public List<GSI.scale> m_scales = new List<GSI.scale>();
		public List<GSI.ammo> m_ammos = new List<GSI.ammo>();
		public List<GSI.paths> m_paths = new List<GSI.paths>();
		public List<GSI.health> m_healths = new List<GSI.health>();
		public List<GSI.arg> m_arguments = new List<GSI.arg>();
		public List<GSI.behaviour> m_behaviours = new List<GSI.behaviour>();
		public List<GSI.map> m_maps = new List<GSI.map>();

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