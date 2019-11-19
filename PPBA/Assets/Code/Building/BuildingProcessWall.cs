using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingProcessWall : MonoBehaviour
{
	private WallRefHolder _holder;
	private bool EnoughResources;

	private void Start()
	{
		_holder = GetComponent<WallRefHolder>();
	}

	public void Startbuilding()
	{
		StartCoroutine(StartBuilding());
	}

	IEnumerator StartBuilding()
	{
		yield return new WaitForSeconds(0.1f);

		while(!EnoughResources)
		{
			if(_holder._BuildingResourceStock >= _holder._BuildingCosts)
			{
				EnoughResources = true;
			}
			yield return new WaitForSeconds(1);
		}

		yield return StartCoroutine(BuildingRoutine());

		_holder.WallMiddlePrefab.SetActive(true);

		yield return null;
	}

	IEnumerator BuildingRoutine()
	{

		// TODO BUIlding zyclus  zb Dissolver
		yield return null;

	}
}
