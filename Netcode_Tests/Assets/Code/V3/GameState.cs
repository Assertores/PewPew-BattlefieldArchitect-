using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NT3 {

	namespace GSI {
		public class type {
			public int m_id;
			public int m_type;
		}

		public class transform {
			public int m_id;
			public float m_x;
			public float m_y;
			public float m_z;
			public float m_alpha;
		}

		public class scale {
			public int m_id;
			public float m_length;
		}

		public class ammo {
			public int m_id;
			public int m_bullets;
			public int m_grenades;
		}

		public class paths {
			public int m_id;
			public List<Vector3> m_path;
		}

		public class health {
			public int m_id;
			public float m_health;
		}
		public class arg {
			public int m_id;
			public byte m_arg;
		}

		public class behaviour {
			public int m_id;
			public byte m_behaviour;
		}

		public class map {
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

		public bool CreateDelta(RingBuffer<GameState> reference, int tick, int length) {
			if (reference == null)
				return false;
			if (reference[tick] == default)
				return false;
			if (reference[tick].m_isDelta)
				return false;

			//--> reference is valide <--

			GameState refState = reference[tick];

			m_refTick = tick;

			foreach(var it in refState.m_types) {
				int index = m_types.FindIndex(x => x.m_id == it.m_id);

				if (it.m_type != m_types[index].m_type)
					continue;

				m_types.RemoveAt(index);
			}
			foreach (var it in refState.m_transforms) {
				int index = m_transforms.FindIndex(x => x.m_id == it.m_id);

				if (it.m_x != m_transforms[index].m_x)
					continue;
				if (it.m_y != m_transforms[index].m_y)
					continue;
				if (it.m_z != m_transforms[index].m_z)
					continue;
				if (it.m_alpha != m_transforms[index].m_alpha)
					continue;

				m_types.RemoveAt(index);
			}
			foreach (var it in refState.m_scales) {
				int index = m_scales.FindIndex(x => x.m_id == it.m_id);

				if (it.m_length != m_scales[index].m_length)
					continue;

				m_types.RemoveAt(index);
			}
			foreach (var it in refState.m_ammos) {
				int index = m_ammos.FindIndex(x => x.m_id == it.m_id);

				if (it.m_bullets != m_ammos[index].m_bullets)
					continue;
				if (it.m_grenades != m_ammos[index].m_grenades)
					continue;

				m_types.RemoveAt(index);
			}
			foreach (var it in refState.m_paths) {
				int index = m_paths.FindIndex(x => x.m_id == it.m_id);

				if (it.m_path.Count != m_paths[index].m_path.Count)
					continue;
				for(int i = 0; i < it.m_path.Count; i++) {
					if (it.m_path[i] != m_paths[index].m_path[i])
						continue;
				}

				m_types.RemoveAt(index);
			}
			foreach (var it in refState.m_healths) {
				int index = m_healths.FindIndex(x => x.m_id == it.m_id);

				if (it.m_health != m_healths[index].m_health)
					continue;

				m_types.RemoveAt(index);
			}
			foreach (var it in refState.m_arguments) {
				int index = m_arguments.FindIndex(x => x.m_id == it.m_id);

				if (it.m_arg != m_arguments[index].m_arg)
					continue;

				m_types.RemoveAt(index);
			}
			foreach (var it in refState.m_behaviours) {
				int index = m_behaviours.FindIndex(x => x.m_id == it.m_id);

				if (it.m_behaviour != m_behaviours[index].m_behaviour)
					continue;

				m_types.RemoveAt(index);
			}
			for(int i = tick; i < tick+length; i++) {
				foreach(var it in reference[i].m_maps) {
					int index = m_maps.FindIndex(x => x.m_id == it.m_id);
					m_maps[i].m_mask += it.m_mask;
				}
			}
			foreach(var it in m_maps) {
				Vector2[] changedPositions = it.m_mask.GetActiveBits();
				it.m_values.Clear();
				foreach(var jt in changedPositions) {
					it.m_values.Add(0.0f);//TODO: read pixel from heatmap
				}
			}

			m_isDelta = true;
			return true;
		}

		public bool DismantleDelta(GameState reference) {
			if (reference == null)
				return false;

			foreach(var it in reference.m_types) {
				if (m_types.Exists(x => x.m_id == it.m_id))
					continue;

				m_types.Add(it);
			}
			foreach (var it in reference.m_transforms) {
				if (m_transforms.Exists(x => x.m_id == it.m_id))
					continue;

				m_transforms.Add(it);
			}
			foreach (var it in reference.m_scales) {
				if (m_scales.Exists(x => x.m_id == it.m_id))
					continue;

				m_scales.Add(it);
			}
			foreach (var it in reference.m_ammos) {
				if (m_ammos.Exists(x => x.m_id == it.m_id))
					continue;

				m_ammos.Add(it);
			}
			foreach (var it in reference.m_paths) {
				if (m_paths.Exists(x => x.m_id == it.m_id))
					continue;

				m_paths.Add(it);
			}
			foreach (var it in reference.m_healths) {
				if (m_healths.Exists(x => x.m_id == it.m_id))
					continue;

				m_healths.Add(it);
			}
			foreach (var it in reference.m_arguments) {
				if (m_arguments.Exists(x => x.m_id == it.m_id))
					continue;

				m_arguments.Add(it);
			}
			foreach (var it in reference.m_behaviours) {
				if (m_behaviours.Exists(x => x.m_id == it.m_id))
					continue;

				m_behaviours.Add(it);
			}
			//maps are somewhat always delta like

			m_isDelta = false;
			return true;
		}

		public bool Lerp(GameState start, GameState end, int t) {
			return false;
		}
	}
}