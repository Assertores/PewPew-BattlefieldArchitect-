using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class CameraStartPosition : MonoBehaviour
	{
		[SerializeField] Transform[] _startPositions;


		private void Start()
		{
#if UNITY_SERVER

			for(int i = 0; i < _startPositions.Length; i++)
			{
				HeatMapCalcRoutine.s_instance.AddSoldier(_startPositions[i], i);
			}
#endif
		}



#if !UNITY_SERVER
		void Update()
		{
			if(!GlobalVariables.s_instance._clients[0]._isConnected)
				return;

			transform.position = _startPositions[GlobalVariables.s_instance._clients[0]._id].position;
			transform.rotation = _startPositions[GlobalVariables.s_instance._clients[0]._id].rotation;

			Destroy(this);
		}
#endif
	}


}