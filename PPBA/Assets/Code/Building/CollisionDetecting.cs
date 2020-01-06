using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PPBA;

public class CollisionDetecting : MonoBehaviour
{
	[SerializeField]private int _FaultBuildingLayer = 9;
	[SerializeField]private Material GhostMaterial;
	
	private Color GhostGreenColor;
	private Color GhostRedColor;

	private bool isCollision = false;

	private void Start()
	{
		GhostGreenColor = BuildingManager.s_instance._ghostGreenColor;
		GhostRedColor = BuildingManager.s_instance._ghostRedColor;
	}

	private void Update()
	{
		if(isCollision)
		{
			BuildingManager.s_instance._canBuild = false;
			GhostMaterial.SetColor("_Color", GhostRedColor);
		}
		else
		{
			BuildingManager.s_instance._canBuild = true;
			GhostMaterial.SetColor("_Color", GhostGreenColor);
		}
	}
	
	private void OnTriggerStay(Collider other)
	{
		if(other.gameObject.layer == _FaultBuildingLayer)
		{
			isCollision = true;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if(other.gameObject.layer == _FaultBuildingLayer)
		{
			isCollision = false;
		}
	}

}
