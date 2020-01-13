using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class CameraStartPosition : MonoBehaviour
	{
		[SerializeField] Transform[] _startPositions;

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