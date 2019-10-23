using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace NT3 {

	enum DataType : byte {NON, TYPE, TRANSFORM, SCALE, AMMO, PATH, HEALTH, ARGUMENT, BEHAVIOUR, MAP };

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
		List<byte[]> m_messageHolder = null;

		public List<byte[]> Encrypt(int maxPackageSize) {
			if (m_messageHolder != null)
				return m_messageHolder;

			List<byte[]> value = new List<byte[]>();
			List<byte> msg = new List<byte>();

			if(m_types.Count > 0) {
				msg.Clear();
				msg.Add((byte)DataType.TYPE);
				msg.AddRange(BitConverter.GetBytes(m_types.Count));
				foreach(var it in m_types) {
					msg.AddRange(BitConverter.GetBytes(it.m_id));
					msg.AddRange(BitConverter.GetBytes(it.m_type));
				}

				HandlePackageSize(maxPackageSize, value, msg.ToArray());
			}
			if (m_transforms.Count > 0) {
				msg.Clear();
				msg.Add((byte)DataType.TRANSFORM);
				msg.AddRange(BitConverter.GetBytes(m_transforms.Count));
				foreach (var it in m_transforms) {
					msg.AddRange(BitConverter.GetBytes(it.m_id));
					msg.AddRange(BitConverter.GetBytes(it.m_x));
					msg.AddRange(BitConverter.GetBytes(it.m_y));
					msg.AddRange(BitConverter.GetBytes(it.m_z));
					msg.AddRange(BitConverter.GetBytes(it.m_alpha));
				}

				HandlePackageSize(maxPackageSize, value, msg.ToArray());
			}
			if (m_scales.Count > 0) {
				msg.Clear();
				msg.Add((byte)DataType.SCALE);
				msg.AddRange(BitConverter.GetBytes(m_scales.Count));
				foreach (var it in m_scales) {
					msg.AddRange(BitConverter.GetBytes(it.m_id));
					msg.AddRange(BitConverter.GetBytes(it.m_length));
				}

				HandlePackageSize(maxPackageSize, value, msg.ToArray());
			}
			if (m_ammos.Count > 0) {
				msg.Clear();
				msg.Add((byte)DataType.AMMO);
				msg.AddRange(BitConverter.GetBytes(m_ammos.Count));
				foreach (var it in m_ammos) {
					msg.AddRange(BitConverter.GetBytes(it.m_id));
					msg.AddRange(BitConverter.GetBytes(it.m_bullets));
					msg.AddRange(BitConverter.GetBytes(it.m_grenades));
				}

				HandlePackageSize(maxPackageSize, value, msg.ToArray());
			}
			if (m_paths.Count > 0) {
				msg.Clear();
				msg.Add((byte)DataType.PATH);
				msg.AddRange(BitConverter.GetBytes(m_paths.Count));
				foreach (var it in m_paths) {
					msg.AddRange(BitConverter.GetBytes(it.m_id));
					msg.AddRange(BitConverter.GetBytes(it.m_path.Count));
					for(int i = 0; i < it.m_path.Count; i++) {
						msg.AddRange(BitConverter.GetBytes(it.m_path[i].x));
						msg.AddRange(BitConverter.GetBytes(it.m_path[i].y));
						msg.AddRange(BitConverter.GetBytes(it.m_path[i].z));
					}
				}

				HandlePackageSize(maxPackageSize, value, msg.ToArray());
			}
			if (m_healths.Count > 0) {
				msg.Clear();
				msg.Add((byte)DataType.HEALTH);
				msg.AddRange(BitConverter.GetBytes(m_healths.Count));
				foreach (var it in m_healths) {
					msg.AddRange(BitConverter.GetBytes(it.m_id));
					msg.AddRange(BitConverter.GetBytes(it.m_health));
				}

				HandlePackageSize(maxPackageSize, value, msg.ToArray());
			}
			if (m_arguments.Count > 0) {
				msg.Clear();
				msg.Add((byte)DataType.ARGUMENT);
				msg.AddRange(BitConverter.GetBytes(m_arguments.Count));
				foreach (var it in m_arguments) {
					msg.AddRange(BitConverter.GetBytes(it.m_id));
					msg.Add(it.m_arg);
				}

				HandlePackageSize(maxPackageSize, value, msg.ToArray());
			}
			if (m_behaviours.Count > 0) {
				msg.Clear();
				msg.Add((byte)DataType.BEHAVIOUR);
				msg.AddRange(BitConverter.GetBytes(m_behaviours.Count));
				foreach (var it in m_behaviours) {
					msg.AddRange(BitConverter.GetBytes(it.m_id));
					msg.Add(it.m_behaviour);
				}

				HandlePackageSize(maxPackageSize, value, msg.ToArray());
			}
			if (m_maps.Count > 0) {
				foreach(var it in m_maps) {
					msg.Clear();
					msg.Add((byte)DataType.MAP);
					msg.AddRange(BitConverter.GetBytes(it.m_id));//overloads the count bytes in compareson to all other types
					msg.AddRange(it.m_mask.ToArray());
					for (int i = 0; i < it.m_values.Count; i++) {
						msg.AddRange(BitConverter.GetBytes(it.m_values[i]));
					}

					HandlePackageSize(maxPackageSize, value, msg.ToArray());
				}
			}

			m_messageHolder = value;

			return value;
		}

		/// <summary>
		/// packs the message into the next best package
		/// </summary>
		/// <param name="maxPackageSize">the maximum size of packages</param>
		/// <param name="packages">the lost of already existing packages</param>
		/// <param name="additionalMessage">the message that should be inserted</param>
		void HandlePackageSize(int maxPackageSize, List<byte[]> packages, byte[] additionalMessage) {
			int index = -1;
			int remainder = int.MaxValue;

			for(int i = 0; i < packages.Count; i++) {
				int currentRefmainter = maxPackageSize - (packages[i].Length + additionalMessage.Length);
				if (currentRefmainter >= 0 && currentRefmainter < remainder) {
					remainder = currentRefmainter;
					index = i;
				}
			}

			if (index < 0) {
				packages.Add(additionalMessage);
			} else {
				byte[] tmp = new byte[packages[index].Length + additionalMessage.Length];
				Buffer.BlockCopy(packages[index], 0, tmp, 0, packages[index].Length);
				Buffer.BlockCopy(additionalMessage, 0, tmp, packages[index].Length, additionalMessage.Length);
				packages[index] = tmp;
			}
		}

		public void Decrypt(byte[] msg, int offset) {

			int count;
			int size;
			while (offset < msg.Length) {
				count = 0;
				count = BitConverter.ToInt32(msg, offset + 1);
				offset += sizeof(int) + 1;
				switch ((DataType)msg[offset - 1 - sizeof(int)]) {
					case DataType.NON:
						Debug.LogError("DataType should not be used!!");
						break;
					case DataType.TYPE:
						m_types = new List<GSI.type>(count);
						for (int i = 0; i < count; i++) {
							m_types.Add(new GSI.type {
								m_id = BitConverter.ToInt32(msg, offset),
								m_type = BitConverter.ToInt32(msg, offset + sizeof(int)),
							});
							offset += 2 * sizeof(int);
						}
						break;
					case DataType.TRANSFORM:
						m_transforms = new List<GSI.transform>(count);
						for (int i = 0; i < count; i++) {
							m_transforms.Add(new GSI.transform {
								m_id = BitConverter.ToInt32(msg, offset),
								m_x = BitConverter.ToSingle(msg, offset + sizeof(int)),
								m_y = BitConverter.ToSingle(msg, offset + sizeof(int) + sizeof(float)),
								m_z = BitConverter.ToSingle(msg, offset + sizeof(int) + 2 * sizeof(float)),
								m_alpha = BitConverter.ToSingle(msg, offset + sizeof(int) + 3 * sizeof(float)),
							});
							offset += sizeof(int) + 4 * sizeof(float);
						}
						break;
					case DataType.SCALE:
						m_scales = new List<GSI.scale>(count);
						for (int i = 0; i < count; i++) {
							m_scales.Add(new GSI.scale {
								m_id = BitConverter.ToInt32(msg, offset),
								m_length = BitConverter.ToSingle(msg, offset + sizeof(int)),
							});
							offset += sizeof(int) + sizeof(float);
						}
						break;
					case DataType.AMMO:
						m_ammos = new List<GSI.ammo>(count);
						for (int i = 0; i < count; i++) {
							m_ammos.Add(new GSI.ammo {
								m_id = BitConverter.ToInt32(msg, offset),
								m_bullets = BitConverter.ToInt32(msg, offset + sizeof(int)),
								m_grenades = BitConverter.ToInt32(msg, offset + 2 * sizeof(int)),
							});
							offset += 3 * sizeof(int);
						}
						break;
					case DataType.PATH:
						m_paths = new List<GSI.paths>(count);
						for(int i = 0; i < count; i++) {
							m_paths[i].m_id = BitConverter.ToInt32(msg, offset);
							offset += sizeof(int);
							size = BitConverter.ToInt32(msg, offset);
							offset += sizeof(int);

							m_paths[i].m_path = new List<Vector3>(size);
							for(int j = 0; j < size; j++) {
								m_paths[i].m_path.Add(new Vector3(BitConverter.ToSingle(msg, offset), BitConverter.ToSingle(msg, offset + sizeof(float)), BitConverter.ToSingle(msg, offset + 2 * sizeof(float))));
								offset += 3 * sizeof(float);
							}
						}
						break;
					case DataType.HEALTH:
						m_healths = new List<GSI.health>(count);
						for (int i = 0; i < count; i++) {
							m_healths.Add(new GSI.health {
								m_id = BitConverter.ToInt32(msg, offset),
								m_health = BitConverter.ToSingle(msg, offset + sizeof(int)),
							});
							offset += sizeof(int) + sizeof(float);
						}
						break;
					case DataType.ARGUMENT:
						m_arguments = new List<GSI.arg>(count);
						for (int i = 0; i < count; i++) {
							m_arguments.Add(new GSI.arg {
								m_id = BitConverter.ToInt32(msg, offset),
								m_arg = msg[offset + sizeof	(int)],
							});
							offset += sizeof(int) + 1;
						}
						break;
					case DataType.BEHAVIOUR:
						m_behaviours = new List<GSI.behaviour>(count);
						for (int i = 0; i < count; i++) {
							m_behaviours.Add(new GSI.behaviour {
								m_id = BitConverter.ToInt32(msg, offset),
								m_behaviour = msg[offset + sizeof(int)],
							});
							offset += sizeof(int) + 1;
						}
						break;
					case DataType.MAP:
						GSI.map tmp = new GSI.map {
							m_id = count, //only case where no count is send, so the bytes that would signify the count of the type array happen to be the id of the map
						};

						byte[] mask = new byte[GlobalValues.s_singelton.m_mapWidth + GlobalValues.s_singelton.m_mapHight];
						Buffer.BlockCopy(msg, offset, mask, 0, mask.Length);
						offset += mask.Length;
						tmp.m_mask = new BitField2D(GlobalValues.s_singelton.m_mapWidth, GlobalValues.s_singelton.m_mapHight, mask);

						size = tmp.m_mask.GetActiveBits().Length;
						tmp.m_values = new List<float>(size);
						for (int j = 0; j < size; j++) {
							tmp.m_values.Add(BitConverter.ToSingle(msg, offset));
							offset += sizeof(float);
						}

						m_maps.Add(tmp);
						break;
					default:
						Debug.LogError("unhandler DataType: " + (DataType)msg[offset - 1]);
						break;
				}
			}
		}

		public bool CreateDelta(RingBuffer<GameState> reference, int tick, int length) {
			if (reference == null)
				return false;
			if (reference[tick] == default)
				return false;
			if (reference[tick].m_isDelta)
				return false;

			//--> reference is valide <--

			m_messageHolder = null;
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

				if (m_maps[i].m_mask.AreAllByteInactive())
					m_maps.RemoveAt(i);
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

			m_messageHolder = null;
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
			foreach(var it in reference.m_maps) {
				if (m_maps.Exists(x => x.m_id == it.m_id))
					continue;

				GSI.map tmp = new GSI.map();
				tmp.m_id = it.m_id;
				tmp.m_mask = new BitField2D(GlobalValues.s_singelton.m_mapHight, GlobalValues.s_singelton.m_mapHight);
				tmp.m_values = new List<float>();

				m_maps.Add(tmp);
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