using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace PPBA {
	public class TickHandler : Singleton<TickHandler>
	{
		public static Action<int> s_DoInput;
		public static Action<int> s_EarlyCalc;
		public static Action<int> s_LateCalc;
		public static Action<int> s_DoTick;
		public static Action<int> s_GatherValues;

		private int _currentTick;
	}
}