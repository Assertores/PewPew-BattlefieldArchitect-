using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class NetworkTeamHandler : MonoBehaviour, INetElement
	{
		IRefHolder refHolder;

		public int _id { get; set; }

		void OnEnable()
		{
			refHolder = GetComponent<IRefHolder>();
#if UNITY_EDITOR
			if(null == refHolder)
			{
				Debug.LogWarning("no Refholder found. self Destruct.");
				Destroy(this);
				return;
			}
#endif

#if UNITY_SERVER
			TickHandler.s_GatherValues += WriteToGameState;
#else
			TickHandler.s_DoInput += ReadFromGameState;
#endif
		}
		private void OnDisable()
		{
#if UNITY_SERVER
			TickHandler.s_GatherValues -= WriteToGameState;
#else
			TickHandler.s_DoInput -= ReadFromGameState;
#endif
		}

		void WriteToGameState(int tick)
		{
			TickHandler.s_interfaceGameState.Add(new GSC.type { _id = _id, _type = 0, _team = (byte)refHolder._team });
		}

		void ReadFromGameState(int tick)
		{
			GSC.type temp = TickHandler.s_interfaceGameState.GetType(_id);

			if(null == temp)
				return;
			if(temp._id != _id)
				return;

			refHolder._team = temp._team;
		}
	}
}