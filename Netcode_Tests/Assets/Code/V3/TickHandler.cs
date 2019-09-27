using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NT3 {
	public class TickHandler : Singelton<TickHandler> {

		uint m_currentTick = 0;

		public static System.Action<uint> s_DoTick = null;

		public void SimulateUptoTick(int maxTick) {

		}

		private void FixedUpdate() {
			
		}
	}
}