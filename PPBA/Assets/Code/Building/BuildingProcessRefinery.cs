using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PPBA;

public class BuildingProcessRefinery : MonoBehaviour
{

	private RefineryRefHolder _holder;
	private bool EnoughResources;

	private void Start()
	{
		_holder = GetComponent<RefineryRefHolder>();
	}

	private void OnEnable()
	{
		Startbuilding();
	}

	private  void Startbuilding()
	{
		StartCoroutine(StartBuilding());
	}

	IEnumerator StartBuilding()
	{
		yield return new WaitForSeconds(0.1f);

		while(!EnoughResources)
		{
			if(_holder._CurrentResources >= _holder._BuildingCosts)
			{
				EnoughResources = true;
			}
			yield return new WaitForSeconds(1);
		}

		yield return StartCoroutine(BuildingRoutine());

		ResourceMapCalculate.s_instance.AddFabric(GetComponent<RefineryRefHolder>());
		_holder.RefineryPrefab.SetActive(true);

		yield return null;
	}

	IEnumerator BuildingRoutine()
	{

		// TODO BUIlding zyclus  zb Dissolver
		yield return null;

	}

}
