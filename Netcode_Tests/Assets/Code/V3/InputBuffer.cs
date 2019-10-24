using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace NT3 {

	public enum ObjectTypes : byte { NON, HQ, RAFINARIE, TRASHWALL, };

	namespace ISI {
		public class paceableObject {
			public int m_id;
			public ObjectTypes m_type;
			public float m_x;
			public float m_y;
			public float m_z;
			public float m_alpha;
		}

		public class wall {
			public int m_id;
			public ObjectTypes m_type;
			public List<Vector3> m_corner;
		}
	}
	public class InputElement {

		List<ISI.paceableObject> m_placeableObjects = new List<ISI.paceableObject>();
		List<ISI.wall> m_walls = new List<ISI.wall>();

		public byte[] Encrypt() {
			List<byte> value = new List<byte>();

			value.AddRange(BitConverter.GetBytes(m_placeableObjects.Count));
			foreach(var it in m_placeableObjects) {
				value.AddRange(BitConverter.GetBytes(it.m_id));
				value.Add((byte)it.m_type);
				value.AddRange(BitConverter.GetBytes(it.m_x));
				value.AddRange(BitConverter.GetBytes(it.m_y));
				value.AddRange(BitConverter.GetBytes(it.m_z));
				value.AddRange(BitConverter.GetBytes(it.m_alpha));
			}

			value.AddRange(BitConverter.GetBytes(m_walls.Count));
			foreach(var it in m_walls) {
				value.AddRange(BitConverter.GetBytes(it.m_id));
				value.Add((byte)it.m_type);

				value.AddRange(BitConverter.GetBytes(it.m_corner.Count));
				for(int i = 0; i < it.m_corner.Count; i++) {
					value.AddRange(BitConverter.GetBytes(it.m_corner[i].x));
					value.AddRange(BitConverter.GetBytes(it.m_corner[i].y));
					value.AddRange(BitConverter.GetBytes(it.m_corner[i].z));
				}
			}

			return value.ToArray();
		}

		/// <summary>
		/// Decypts an byte-array to usable data
		/// </summary>
		/// <param name="msg">the byte-array</param>
		/// <param name="offset">the start offset</param>
		/// <returns>the offset at with point i stopt to read</returns>
		public int Decrypt(byte[] msg, int offset) {
			int count = BitConverter.ToInt32(msg, offset);
			offset += sizeof(int);
			for (int i = 0; i < count; i++) {
				ISI.paceableObject tmp = new ISI.paceableObject {
					m_id = BitConverter.ToInt32(msg, offset),
					m_type = (ObjectTypes)msg[offset + sizeof(int)],
					m_x = BitConverter.ToSingle(msg, offset + sizeof(int) + 1),
					m_y = BitConverter.ToSingle(msg, offset + sizeof(int) + 1 + sizeof(float)),
					m_z = BitConverter.ToSingle(msg, offset + sizeof(int) + 1 + 2 * sizeof(float)),
					m_alpha = BitConverter.ToSingle(msg, offset + sizeof(int) + 1 + 3 * sizeof(float)),
				};
				offset += sizeof(int) + 1 + 4 * sizeof(float);
			}

			count = BitConverter.ToInt32(msg, offset);
			offset += sizeof(int);
			for (int i = 0; i < count; i++) {
				ISI.wall tmp = new ISI.wall {
					m_id = BitConverter.ToInt32(msg, offset),
					m_type = (ObjectTypes)msg[offset + sizeof(int)],
				};
				offset += sizeof(int) + 1;

				int size = BitConverter.ToInt32(msg, offset);
				offset += sizeof(int);

				tmp.m_corner = new List<Vector3>(size);
				for(int j = 0; j < size; i++) {
					tmp.m_corner.Add(new Vector3(BitConverter.ToSingle(msg, offset),
												 BitConverter.ToSingle(msg, offset + sizeof(float)),
												 BitConverter.ToSingle(msg, offset + 2 * sizeof(float))));
					offset += 3 * sizeof(float);
				}
			}

			return offset;
		}

		public List<int> GetInputIDs() {
			List<int> values = new List<int>();

			foreach(var it in m_placeableObjects) {
				values.Add(it.m_id);
			}
			foreach(var it in m_walls) {
				values.Add(it.m_id);
			}

			return values;
		}
	}

	public class InputBuffer : RingBuffer<InputElement> {

		public int m_iD = -1;
		public InputElement m_currentInputElement { get; private set; } = new InputElement();

		public new InputElement this[int key] {
			get => base[key];
			private set => base[key] = value;
		}

		public void CreateNewElement(int tick) {
			if (tick < GetLowEnd() || tick > GetHighEnd())
				return;

			InputElement element = new InputElement();
			this[tick + GlobalValues.s_singelton.m_lockStepBufferSize] = element;
			m_currentInputElement = element;
		}

		public void AddNewElement(InputElement value, int tick) {
			if (this[tick] != default)
				return;

			this[tick] = value;
			m_currentInputElement = value;
		}
	}
}