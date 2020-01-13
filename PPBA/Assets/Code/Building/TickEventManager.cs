using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class TickEventManager : Singleton<TickEventManager>
	{
		List<GSC.input> _denyedInputs = new List<GSC.input>();

		private void Start()
		{
#if UNITY_SERVER
			TickHandler.s_DoInput += TranslateInputToGame;
			TickHandler.s_GatherValues += ReturnDenyedInputs;
#endif
		}
		private void OnDestroy()
		{
#if UNITY_SERVER
			TickHandler.s_DoInput -= TranslateInputToGame;
			TickHandler.s_GatherValues -= ReturnDenyedInputs;
#endif
		}

		void TranslateInputToGame(int tick)
		{
			foreach(var it in TickHandler.s_interfaceInputState._objs)
			{
				if(null == GlobalVariables.s_instance._prefabs[(int)it._type] || !ObjectPool.s_objectPools.ContainsKey(GlobalVariables.s_instance._prefabs[(int)it._type]))
					continue;

				if(false)//TODO: prüfen ob es an der stelle gesetzt werden darf
				{
					_denyedInputs.Add(new GSC.input { _id = it._id, _client = it._client });
				}

				MonoBehaviour element = ObjectPool.s_objectPools[GlobalVariables.s_instance._prefabs[(int)it._type]].GetNextObject(it._client);

				element.transform.position = it._pos;
				element.transform.rotation = Quaternion.Euler(0, it._angle, 0);
			}
			foreach(var it in TickHandler.s_interfaceInputState._combinedObjs)
			{
				if(null == GlobalVariables.s_instance._prefabs[(int)it._type] || !ObjectPool.s_objectPools.ContainsKey(GlobalVariables.s_instance._prefabs[(int)it._type]))
					continue;

				if(false)//TODO: prüfen ob es an der stelle gesetzt werden darf
				{
					_denyedInputs.Add(new GSC.input { _id = it._id, _client = it._client });
				}

				MonoBehaviour element = ObjectPool.s_objectPools[GlobalVariables.s_instance._prefabs[(int)it._type]].GetNextObject(it._client);

				element.transform.position = it._corners[0];
				for(int i = 1; i < it._corners.Length; i++)
				{
					//----- ----- corner ----- -----
					element = ObjectPool.s_objectPools[GlobalVariables.s_instance._prefabs[(int)it._type]].GetNextObject();
					if(element is IRefHolder)
					{
						(element as IRefHolder)._team = it._client;
					}

					element.transform.position = it._corners[i];
					//----- ----- between ----- -----
					element = ObjectPool.s_objectPools[GlobalVariables.s_instance._prefabs[(int)it._type + 1]].GetNextObject();
					element.transform.position = (it._corners[i - 1] + it._corners[i]) / 2;
					element.transform.rotation = Quaternion.LookRotation(it._corners[i - 1] - it._corners[i], Vector3.up);
				}
			}
		}

		void ReturnDenyedInputs(int tick)
		{
			TickHandler.s_interfaceGameState._denyedInputIDs.AddRange(_denyedInputs);
			_denyedInputs.Clear();
		}
	}
}

