using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class HeadQuarter : MonoBehaviour
	{
		#region Variables
		[Header("CarePackage")]
		[SerializeField] private int _suppliesPerTick = 1;
		[SerializeField] private int _ammoPerTick = 1;
		#endregion

		#region References
		[SerializeField] public ResourceDepot _resourceDepot;
		#endregion

		private void CarePackage(int tick = 0)
		{
			_resourceDepot.GiveResources(_suppliesPerTick);
			_resourceDepot.GiveAmmo(_ammoPerTick);
		}

		private void SpawnPawn(int tick = 0)
		{
			if(TickHandler.s_currentTick % 100 == 50)
				Pawn.Spawn(Pawn.RandomPawnType(), transform.position, _resourceDepot._team);
		}

		private void OnEnable()
		{
#if UNITY_SERVER
			TickHandler.s_DoTick += CarePackage;
			TickHandler.s_DoTick += SpawnPawn;

			if(null != JobCenter.s_headQuarters[_resourceDepot._team])
				if(!JobCenter.s_headQuarters[_resourceDepot._team].Contains(this))
					JobCenter.s_headQuarters[_resourceDepot._team].Add(this);
#endif
		}

		private void OnDisable()
		{
#if UNITY_SERVER
			TickHandler.s_DoTick -= CarePackage;
			TickHandler.s_DoTick -= SpawnPawn;

			if(null != JobCenter.s_headQuarters[_resourceDepot._team])
				if(JobCenter.s_headQuarters[_resourceDepot._team].Contains(this))
					JobCenter.s_headQuarters[_resourceDepot._team].Remove(this);
#endif
		}
	}
}