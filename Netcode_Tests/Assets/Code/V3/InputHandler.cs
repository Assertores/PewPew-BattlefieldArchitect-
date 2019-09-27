using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NT3 {
	public class InputHandler : MonoBehaviour {

		public static List<InputHandler> m_refList = new List<InputHandler>();

		public int m_iD = -1;
		[SerializeField] bool m_onlyClientSide = false;

		public InputBuffer m_inputBuffer { get; private set; } = new InputBuffer();

		private void Awake() {
#if UNITY_SERVER
            if (onlyClientSide) {
                Destroy(this);
                return;
            }
#endif
			m_refList.Add(this);
		}

		private void OnDestroy() {
			m_refList.Remove(this);
		}

		
	}
}