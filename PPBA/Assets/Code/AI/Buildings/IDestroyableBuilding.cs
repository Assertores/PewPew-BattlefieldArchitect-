using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public interface IDestroyableBuilding
	{
		int _id { get; set; }

		void TakeDamage(int amount);
		Transform GetTransform();
		float GetHealth();
		float GetMaxHealth();
		int GetTeam();
	}
}