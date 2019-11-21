using System.Collections;
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

		public ObjectType _type;
		int _id;
		int _tick;

		void ServerInputHandling(int tick)
		{
			Debug.Log("server executes inputs from clients");
		}


		// bestätigung ob die übergebenen ip funktioniert hat aus s_interfaceGameState
		void ClientInputHandling(int tick)
		{
			if(tick == _tick)
			{
				if(TickHandler.s_interfaceGameState._denyedInputIDs.Exists(x => x == _id))
				{
					Debug.Log("input was denyed");
				}
				else
				{
					Debug.Log("input was sucsessfull");
				}
				Destroy(this.gameObject);
			}
		}

		void ServerInputGather(int tick)
		{
			Debug.Log("server gathers all denyed inputs");
		}


		// hier schreiben in den s_interfaceInputState und bekommst id zurück
		void ClientInputGather(int tick)
		{

			Debug.Log("client writes all inputs since last tick into inputstate");
			_tick = tick;
			_id = TickHandler.s_interfaceInputState.AddObj(_type, transform.position, transform.rotation.eulerAngles.y);
		}
	}
}