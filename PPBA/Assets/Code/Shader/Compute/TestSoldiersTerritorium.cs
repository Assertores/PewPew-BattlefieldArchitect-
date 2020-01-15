using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PPBA
{

	public class TestSoldiersTerritorium : MonoBehaviour
	{
		public Transform point1;
		public Transform point2;
		public Transform target;
		public int team = 0;

		void Start()
		{
			target = point1;

			TerritoriumMapCalculate.s_instance.AddSoldier(this.transform, team);

		}


		void Update()
		{
			float dist = (target.position - transform.position).magnitude;

			if(dist <= 0.5)
			{
				if(target == point1)
				{
					target = point2;
				}
				else
				{
					target = point1;
				}
			}

			transform.position = Vector3.MoveTowards(transform.position, target.position, 1);

			TerritoriumMapCalculate.s_instance.StartCalculation();
		}
	}
}
