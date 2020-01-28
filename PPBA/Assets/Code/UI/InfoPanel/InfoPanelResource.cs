using System.Collections;
using UnityEngine;
using TMPro;

namespace PPBA
{
	public class InfoPanelResource : MonoBehaviour
	{
		public ObjectType _Typ;
		public TextMeshProUGUI text;
		[SerializeField] float RefreshRate = 1;
		/// <summary>
		/// bool is for resources update 
		/// </summary>
		public bool selfUpdate = false;

		// Start is called before the first frame update
		void Start()
		{
			if(!selfUpdate)
			{
				BuildingManager.s_instance._InfoPanelEvent += RefreshPanel;
			}
			else
			{
				StartCoroutine(RefreshResources());
			}
		}

		private void RefreshPanel(ObjectType _type)
		{
			if(_Typ != _type)
			{
				return;
			}

			if(null == text)
				return;

			switch(_type)
			{
				case ObjectType.REFINERY:
					text.text = GetComponent<BuildingManager>()._refineriesHolder.Count.ToString();
					break;
				case ObjectType.DEPOT:
					text.text = GetComponent<BuildingManager>()._depotHolder.Count.ToString();
					break;
				case ObjectType.PAWN_WARRIOR:
					break;
				case ObjectType.PAWN_HEALER:
					break;
				case ObjectType.PAWN_PIONEER:
					break;
				case ObjectType.MEDICAMP:
					text.text = GetComponent<BuildingManager>()._mediCampHolder.Count.ToString();
					break;
				case ObjectType.SIZE:
					break;
			}
		}

		IEnumerator RefreshResources()
		{
			while(true)
			{
				text.text =  ResourceDepot._resourceTotal[GlobalVariables.s_instance._clients[0]._id].ToString();
				yield return new WaitForSeconds(RefreshRate);
			//	float res = JobCenter.GetResourceTotal(GlobalVariables.s_instance._clients[0]._id);
			//	if(null != text)
					//text.text = res.ToString();
			}
		}

	}
}
