using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class DebugController : MonoBehaviour
	{
		[SerializeField] private bool _printAllClientGamestates;

		void Start()
		{
//#if !UNITY_SERVER
			TickHandler.s_DoTick += PrintGameState;
//#endif
		}

		private void OnDestroy()
		{
			TickHandler.s_DoTick -= PrintGameState;
		}

		void Update()
		{

		}

		private void PrintGameState(int tick = 0) => Debug.Log(TickHandler.s_interfaceGameState.ToString());
	}
}