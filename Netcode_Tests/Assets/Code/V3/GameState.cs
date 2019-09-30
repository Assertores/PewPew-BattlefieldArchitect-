using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NT3 {
	public class GameState {

		public int RefTick = -1;
		public byte[] Encrypt() {
			return null;
		}

		public void Decrypt(byte[] msg, int offset) {

		}

		public bool CreateDelta(GameState reference) {
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