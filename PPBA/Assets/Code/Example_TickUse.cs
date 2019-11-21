﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class Example_TickUse : MonoBehaviour
	{

		private void OnEnable()
		{
#if UNITY_SERVER
			TickHandler.s_DoInput += ServerInputHandling;
			TickHandler.s_GatherValues += ServerInputGather;
#else
			TickHandler.s_DoInput += ClientInputHandling;
			TickHandler.s_GatherValues += ClientInputGather;
#endif
		}

		private void OnDisable()
		{
#if UNITY_SERVER
			TickHandler.s_DoInput -= ServerInputHandling;
			TickHandler.s_GatherValues -= ServerInputGather;
#else
			TickHandler.s_DoInput -= ClientInputHandling;
			TickHandler.s_GatherValues -= ClientInputGather;
#endif
		}

		void ServerInputHandling(int tick)
		{
			Debug.Log("server executes inputs from clients");
		}


		// bestätigung ob die übergebenen ip funktioniert hat aus s_interfaceGameState
		void ClientInputHandling(int tick)
		{
			Debug.Log("client executes or denyes inputs");
		}

		void ServerInputGather(int tick)
		{
			Debug.Log("server gathers all denyed inputs");
		}


		// hier schreiben in den s_interfaceInputState und bekommst id zurück
		void ClientInputGather(int tick)
		{
			Debug.Log("client writes all inputs since last tick into inputstate");
		}
	}
}