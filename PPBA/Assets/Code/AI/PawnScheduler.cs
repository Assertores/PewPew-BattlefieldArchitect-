using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class PawnScheduler : MonoBehaviour
	{
		private void Awake()
		{
#if UNITY_SERVER
			TickHandler.s_DoInput += ReadFromInputState;
			TickHandler.s_GatherValues += WriteToGameState;
#else
			TickHandler.s_DoInput += ReadFroomGameState;
#endif
		}

		private void OnDestroy()
		{
#if UNITY_SERVER
			TickHandler.s_DoInput -= ReadFromInputState;
			TickHandler.s_GatherValues -= WriteToGameState;
#else
			TickHandler.s_DoInput -= ReadFroomGameState;
#endif
		}

		void ReadFromInputState(int tick)
		{
			foreach (var it in TickHandler.s_interfaceInputState._produceUnits)
			{
				GlobalVariables.s_instance._clients.Find(x => x._id == it._client)._scheduledPawns[it._pawnType] = it._pawnCount;
			}
		}

		void WriteToGameState(int tick)
		{
			foreach(var it in GlobalVariables.s_instance._clients)
			{
				for(byte i = 0; i < it._scheduledPawns.Length; i++)
				{
					TickHandler.s_interfaceGameState.Add(new GSC.sheduledPawns { _id = it._id, _type = i, _count = (byte)it._scheduledPawns[i] });
				}
			}
		}

		void ReadFroomGameState(int tick)
		{
			client me = GlobalVariables.s_instance._clients[0];

			List<GSC.sheduledPawns> tmp = TickHandler.s_interfaceGameState._scheduledPawns.FindAll(x => x._id == me._id);
			foreach(var it in tmp)
			{
				me._scheduledPawns[it._type] = it._count;
			}
		}
	}
}
