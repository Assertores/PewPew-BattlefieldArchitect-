using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PPBA;

public class CollisionDetecting : MonoBehaviour
{
	[SerializeField]private int _FaultBuildingLayer = 9;
	[SerializeField]private Material GhostMaterial;

	private void OnTriggerStay(Collider other)
	{
		if(other.gameObject.layer == _FaultBuildingLayer)
		{
			//BuildingManager.s_instance._canBuild = false;

			if(GhostMaterial != null)
			{
				GhostMaterial.SetColor("_Color", new Color(1,0,0,0.3f));
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if(other.gameObject.layer == _FaultBuildingLayer)
		{
			//BuildingManager.s_instance._canBuild = true;
			if(GhostMaterial != null)
			{
				GhostMaterial.SetColor("_Color", new Color(0, 1, 0, 0.3f));
			}

		}
	}
}
