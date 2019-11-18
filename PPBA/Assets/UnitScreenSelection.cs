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
		TextMeshProUGUI test1 = new TextMeshProUGUI();
		TextMeshProUGUI test2 = new TextMeshProUGUI();
		test1.text = "Resources " + GetComponent<RefineryRefHolder>()._CurrentResources.ToString();
		test2.text = "Live " + GetComponent<RefineryRefHolder>()._LivePoints.ToString();

		UnitScreenController.s_instance.AddUnitInfoPanel(gameObject.transform, test1.text , test2.text, ref _UnitDetails);
		UnitScreenUpdate(true);
	}

	public void UnitScreenUpdate( bool UpdateScreen)
	{
		if(UpdateScreen == true)
		{
			StartCoroutine(UnitScreeenUpdateRoutine());
		}
		else
		{
			StopCoroutine(UnitScreeenUpdateRoutine());
		}
	}

	IEnumerator UnitScreeenUpdateRoutine()
	{
		while(true)
		{
			_UnitDetails[0].text = "Resources " + GetComponent<RefineryRefHolder>()._CurrentResources.ToString();
			_UnitDetails[1].text = "Live " + GetComponent<RefineryRefHolder>()._LivePoints.ToString();

			yield return new WaitForSeconds(_ScreenRefreshTime);
		}
	}
}
