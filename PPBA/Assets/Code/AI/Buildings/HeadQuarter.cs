using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class HeadQuarter : MonoBehaviour//, INetElement
	{
		#region Variables

		#endregion

		#region References
		[SerializeField] private ResourceDepot _resourceDepot;
		#endregion

		private void CarePackage(int tick = 0)
		{

		}

		private void OnEnable()
		{
#if UNITY_SERVER
			if(null != JobCenter.s_headQuarters[_resourceDepot._team])
				if(!JobCenter.s_headQuarters[_resourceDepot._team].Contains(this))
					JobCenter.s_headQuarters[_resourceDepot._team].Add(this);
#endif
		}

		private void OnDisable()
		{
#if UNITY_SERVER
			if(null != JobCenter.s_headQuarters[_resourceDepot._team])
				if(JobCenter.s_headQuarters[_resourceDepot._team].Contains(this))
					JobCenter.s_headQuarters[_resourceDepot._team].Remove(this);
#endif
		}
	}
}