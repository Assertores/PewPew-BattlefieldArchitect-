using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NT3 {
	public class InputElement {


		public byte[] Encrypt() {
			return null;
		}

		/// <summary>
		/// Decypts an byte-array to usable data
		/// </summary>
		/// <param name="msg">the byte-array</param>
		/// <param name="offset">the start offset</param>
		/// <returns>the offset at with point i stopt to read</returns>
		public int Decrypt(byte[] msg, int offset) {
			return offset;
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