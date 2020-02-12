using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class DebugNearestBoarder : MonoBehaviour
	{
#if UNITY_EDITOR

		Vector3 _target = new Vector3();

		private void Start()
		{
			TickHandler.s_DoTick += UpdateTarget;
		}

		private void OnDestroy()
		{
			TickHandler.s_DoTick -= UpdateTarget;
		}

		private void Update()
		{
			Debug.DrawLine(transform.position, _target, Color.magenta);
		}

		void UpdateTarget(int tick)
		{
			Vector3 tmp = HeatMapHandler.s_instance.BorderValues(transform.position);

			_target = new Vector3(tmp.x, transform.position.y, tmp.y);
		}
#endif
	}
}