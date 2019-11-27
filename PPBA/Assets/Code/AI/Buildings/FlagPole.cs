using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class FlagPole : MonoBehaviour, INetElement
	{
		public int _id { get; set; }
		public int _team = 0;

		void Start()
		{

		}

		void Update()
		{

		}

		public static void Spawn(Vector3 spawnPoint, int team)
		{
			FlagPole newFlagPole = (FlagPole)ObjectPool.s_objectPools[GlobalVariables.s_instance._prefabs[(int)ObjectType.FLAGPOLE]].GetNextObject();
			newFlagPole.transform.position = spawnPoint;
			newFlagPole._team = team;

			JobCenter.s_flagPoles[team].Add(newFlagPole);
		}

		private void OnDisable()
		{
			if(JobCenter.s_flagPoles[_team].Contains(this))
				JobCenter.s_flagPoles[_team].Remove(this);
		}
	}
}