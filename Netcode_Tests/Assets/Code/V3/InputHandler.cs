using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NT3 {
	public class InputHandler : Singelton<InputHandler> {

		private void Start() {
#if UNITY_SERVER
            Destroy(this);
            return;
#endif
		}
	}
}