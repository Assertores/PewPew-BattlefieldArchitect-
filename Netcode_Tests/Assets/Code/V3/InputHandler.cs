using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NT3 {
	public class InputHandler : MonoBehaviour {

		[SerializeField] bool m_onlyClientSide = false;

		public InputBuffer m_inputBuffer { get; private set; } = new InputBuffer();

#if UNITY_SERVER
		private void Awake() {
            if (onlyClientSide) {
                Destroy(this);
                return;
            }
		}
#endif
	}
}