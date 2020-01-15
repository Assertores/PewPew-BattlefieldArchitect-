using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public interface IDestroyableBuilding
	{
		public void TakeDamage(int amount);
	}
}