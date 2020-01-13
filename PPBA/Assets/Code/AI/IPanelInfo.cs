using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace PPBA
{
	public interface IPanelInfo//let all clickable objects implement this interface.
	{
		void InitialiseUnitPanel();//Calls UnitScreenController.AddUnitInfoPanel for each value shown.
		void UpdateUnitPanelInfo();//Use the refs provided by AddUnitInfoPanel to refresh the values in the panel.
		
		//ALSO ADD:
		//private TextMeshProUGUI[] _panelDetails;
	}
}