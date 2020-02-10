using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PPBA
{
	public class FlagBuild : MonoBehaviour
	{
		public GameObject _Flag;

		public void BuildFlag()
		{
			BuildingManager.s_instance.HandleNewObject(_Flag.GetComponent<IRefHolder>());
		}

	}
}
