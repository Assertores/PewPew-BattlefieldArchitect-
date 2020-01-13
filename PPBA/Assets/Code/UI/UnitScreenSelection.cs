using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PPBA;
using TMPro;

public class UnitScreenSelection : MonoBehaviour
{
	public float _ScreenRefreshTime = 0.5f;
	public TextMeshProUGUI[] _UnitDetails;
	
	void OnMouseDown()
	{
		//string test1 = "Resources " + GetComponent<RefineryRefHolder>()._CurrentResources.ToString();
		//string test2 = "Live " + GetComponent<RefineryRefHolder>()._LivePoints.ToString();

		//UnitScreenController.s_instance.AddUnitInfoPanel(gameObject.transform, test1 , test2, ref _UnitDetails);
		//UnitScreenUpdate(true);
	}

	//public void UnitScreenUpdate( bool UpdateScreen)
	//{
	//	if(UpdateScreen == true)
	//	{
	//		StartCoroutine(UnitScreeenUpdateRoutine());
	//	}
	//	else
	//	{
	//		StopCoroutine(UnitScreeenUpdateRoutine());
	//	}
	//}

	//IEnumerator UnitScreeenUpdateRoutine()
	//{
	//	while(true)
	//	{
	//		_UnitDetails[0].text = "Resources " + GetComponent<RefineryRefHolder>()._CurrentResources.ToString();
	//		_UnitDetails[1].text = "Live " + GetComponent<RefineryRefHolder>()._LivePoints.ToString();

	//		yield return new WaitForSeconds(_ScreenRefreshTime);
	//	}
	//}
}
