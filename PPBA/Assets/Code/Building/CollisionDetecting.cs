using UnityEngine;
using PPBA;

public class CollisionDetecting : MonoBehaviour
{
	[SerializeField]private int _FaultBuildingLayer;

	private void OnTriggerStay(Collider other)
	{
		if(other.gameObject.layer == _FaultBuildingLayer)
		{
			BuildingController.s_instance._canBuild = false;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if(other.gameObject.layer == _FaultBuildingLayer)
		{
			BuildingController.s_instance._canBuild = true;
		}
	}
}
