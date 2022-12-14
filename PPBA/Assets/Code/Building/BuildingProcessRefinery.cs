using System.Collections;
using UnityEngine;

namespace PPBA
{
	public class BuildingProcessRefinery : MonoBehaviour
	{
		private IRefHolder element;
		private bool _EnoughResources;

		private void OnEnable()
		{
#if !UNITY_SERVER
			element = GetComponent<IRefHolder>();
			Startbuilding();
#endif
		}

		private void Startbuilding()
		{
			StartCoroutine(StartBuilding());
		}

		IEnumerator StartBuilding()
		{
			yield return new WaitForSeconds(0.01f);

			while(!_EnoughResources)
			{
				//if(element.GetBuildingCurrentResources() >= element.GetBuildingCost())
				//{
				//	_EnoughResources = true;
				//}
				//yield return new WaitForSeconds(1);
			}

			yield return StartCoroutine(BuildingRoutine());


			// build is fished
			switch(element.GetObjectType())
			{
				case ObjectType.REFINERY:
					HeatMapCalcRoutine.s_instance.AddFabric(GetComponent<RefineryRefHolder>());
					break;
				case ObjectType.DEPOT:
					break;
				case ObjectType.GUN_TURRET:
					break;
				case ObjectType.WALL:
					break;
				case ObjectType.WALL_BETWEEN:
					break;
				case ObjectType.PAWN_WARRIOR:
					break;
				case ObjectType.PAWN_HEALER:
					break;
				case ObjectType.PAWN_PIONEER:
					break;
				case ObjectType.COVER:
					break;
				case ObjectType.FLAGPOLE:
					break;
				case ObjectType.SIZE:
					break;
				default:
					break;
			}


			yield return null;
		}

		IEnumerator BuildingRoutine()
		{

			// TODO BUIlding zyclus  zb Dissolver
			yield return null;

		}

	}
}
