using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class TickBuildEmitter : MonoBehaviour
	{
		public ObjectType _type;
		private int _tick;
		private int _id;

		public void AddToGatherValue()
		{
#if !UNITY_SERVER
			TickHandler.s_GatherValues += RegisterInput;
#endif
		}
		private void OnDestroy()
		{
			TickHandler.s_DoInput -= ReactOnInput;
		}

		void RegisterInput(int tick)
		{
			TickHandler.s_GatherValues -= RegisterInput;
			TickHandler.s_DoInput += ReactOnInput;

			_tick = tick;
			_id = TickHandler.s_interfaceInputState.AddObj(_type,transform.position,transform.eulerAngles.y);
		}

		void ReactOnInput(int tick)
		{
			if(tick == _tick)
			{
				TickHandler.s_DoInput -= ReactOnInput;
				
				if(TickHandler.s_interfaceGameState._denyedInputIDs.Exists(x => x._id == _id))
				{
					//TODO: eingabe war invalide
				}
				else
				{
					//TickHandler.s_currentTick.
					//TODO: eingabe war valide
				}
				if(null != this.gameObject)
					Destroy(this.gameObject);
			}
		}
	}
}