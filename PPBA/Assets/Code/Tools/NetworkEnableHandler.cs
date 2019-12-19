using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class NetworkEnableHandler : MonoBehaviour, INetElement
	{
		public int _id { get; set; }

		void Start()
		{
#if UNITY_SERVER
			TickHandler.s_GatherValues += WriteToGameState;
#else
			TickHandler.s_DoInput += ReadFromGameState;
#endif
		}
		private void OnDestroy()
		{
#if UNITY_SERVER
			TickHandler.s_GatherValues -= WriteToGameState;
#else
			TickHandler.s_DoInput -= ReadFromGameState;
#endif
		}

		void WriteToGameState(int tick)
		{
			GSC.arg temp = TickHandler.s_interfaceGameState.GetArg(_id);
			if(null == temp)
			{
				temp = new GSC.arg();
				TickHandler.s_interfaceGameState._args.Add(temp);
			}

			if(gameObject.activeSelf)
				temp._arguments |= Arguments.ENABLED;
		}

		void ReadFromGameState(int tick)
		{
			GSC.arg temp = TickHandler.s_interfaceGameState.GetArg(_id);
			if(null == temp)
				return;

			gameObject.SetActive(temp._arguments.HasFlag(Arguments.ENABLED));
		}
	}
}