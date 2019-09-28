using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NT3 {
	public class InputElement {


		public byte[] Encrypt() {
			return null;
		}

		public void Decrypt(byte[] msg, int offset) {

		}
	}

	public class InputBuffer : RingBuffer<InputElement> {

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