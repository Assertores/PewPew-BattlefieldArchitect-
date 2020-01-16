using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace PPBA
{
	public class InfoPanelResource : MonoBehaviour
	{
		public ObjectType _Typ;
		public TextMeshProUGUI text; 

		// Start is called before the first frame update
		void Start()
		{
			BuildingManager.s_instance._InfoPanelEvent += RefreshPanel;
		}

		private void RefreshPanel(ObjectType _type)
		{
			if(_Typ != _type)
			{
				return;
			}

			switch(_type)
			{
				case ObjectType.REFINERY:
					text.text = GetComponent<BuildingManager>()._refineriesHolder.Count.ToString();
					break;
				case ObjectType.DEPOT:
					text.text = GetComponent<BuildingManager>()._refineriesHolder.Count.ToString();
					break;
				case ObjectType.PAWN_WARRIOR:
					break;
				case ObjectType.PAWN_HEALER:
					break;
				case ObjectType.PAWN_PIONEER:
					break;
				case ObjectType.MEDICAMP:
					text.text = GetComponent<BuildingManager>()._refineriesHolder.Count.ToString();
					break;
				case ObjectType.SIZE:
					break;
			}
		}
	}
}
