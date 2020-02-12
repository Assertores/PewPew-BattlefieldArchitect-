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
				if(_Typ == ObjectType.PAWN_HEALER ||
					_Typ == ObjectType.PAWN_PIONEER ||
					_Typ == ObjectType.PAWN_WARRIOR)
				{
					Pawn.PawnChainged += RefreshPanel;
				}
				else
				{
					BuildingManager.s_instance._InfoPanelEvent += RefreshPanel;
				}
			}
			else
			{
				StartCoroutine(RefreshResources());
			}
		}

		private void OnDestroy()
		{
			if(_Typ == ObjectType.PAWN_HEALER ||
					_Typ == ObjectType.PAWN_PIONEER ||
					_Typ == ObjectType.PAWN_WARRIOR)
			{
				Pawn.PawnChainged -= RefreshPanel;
			}
			else
			{
				BuildingManager.s_instance._InfoPanelEvent -= RefreshPanel;
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
					text.text = BuildingManager.s_instance._refineriesHolder.FindAll(x => x._team == GlobalVariables.s_instance._clients[0]._id).Count.ToString();
					break;
				case ObjectType.DEPOT:
					text.text = BuildingManager.s_instance._depotHolder.FindAll(x => x._team == GlobalVariables.s_instance._clients[0]._id).Count.ToString();
					break;
				case ObjectType.PAWN_WARRIOR:
					text.text = Pawn.GetActivePawnTypes(GlobalVariables.s_instance._clients[0]._id).x.ToString();
					break;
				case ObjectType.PAWN_HEALER:
					text.text = Pawn.GetActivePawnTypes(GlobalVariables.s_instance._clients[0]._id).y.ToString();
					break;
				case ObjectType.PAWN_PIONEER:
					text.text = Pawn.GetActivePawnTypes(GlobalVariables.s_instance._clients[0]._id).z.ToString();
					break;
				case ObjectType.MEDICAMP:
					text.text = BuildingManager.s_instance._mediCampHolder.FindAll(x => x._team == GlobalVariables.s_instance._clients[0]._id).Count.ToString();
					break;
				case ObjectType.SIZE:
					break;
				default:
					break;
			}
		}

		IEnumerator RefreshResources()
		{
			while(true)
			{
				text.text = ResourceDepot._resourceTotal[GlobalVariables.s_instance._clients[0]._id].ToString();
				yield return new WaitForSeconds(RefreshRate);
				//	float res = JobCenter.GetResourceTotal(GlobalVariables.s_instance._clients[0]._id);
				//	if(null != text)
				//text.text = res.ToString();
			}
		}

	}
}
