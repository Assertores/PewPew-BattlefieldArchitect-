using UnityEngine;
using DG.Tweening;
using TMPro;

namespace PPBA
{
	public class UnitScreenController : Singleton<UnitScreenController>
	{

		public RectTransform UnitPanel;
		public Transform _UnitDetailPanel;

		public GameObject _UnitDetailPrefab;
		public Transform _UnitCamera;
		private bool _isUnitPanel = false;
		private TextMeshProUGUI[] _currentUnitDetails;

		public void MoveUnitPanel()
		{
			if(_isUnitPanel)
			{
				UnitPanel.DOAnchorPos(new Vector2(-UnitPanel.rect.x, 400), 0.25f);
			}
			else
			{
				UnitPanel.DOAnchorPos(new Vector2(-UnitPanel.rect.x, 0), 0.25f);
			}

			_isUnitPanel = !_isUnitPanel;
		}

		public void AddUnitInfoPanel(Transform TargetUnitCamera, string TextForUnitPanel, ref TextMeshProUGUI[] TMProTExt)
		{
			if(_currentUnitDetails != TMProTExt)
			{
				string[] textinfo = new string[1];
				textinfo[0] = TextForUnitPanel;
				CreateDetail(textinfo, ref TMProTExt);
				SetCamera(TargetUnitCamera);
			}

		}

		public void AddUnitInfoPanel(Transform TargetUnitCamera, string TextForUnitPanel, string TextForUnitPanel1, ref TextMeshProUGUI[] TMProTExt)
		{
			if(_currentUnitDetails != TMProTExt)
			{
				string[] textinfo = new string[2];
				textinfo[0] = TextForUnitPanel;
				textinfo[1] = TextForUnitPanel1;
				CreateDetail(textinfo, ref TMProTExt);
				SetCamera(TargetUnitCamera);
			}
		}

		public void AddUnitInfoPanel(Transform TargetUnitCamera, string TextForUnitPanel, string TextForUnitPanel1, string TextForUnitPanel2, ref TextMeshProUGUI[] TMProTExt)
		{
			if(_currentUnitDetails != TMProTExt)
			{
				string[] textinfo = new string[3];
				textinfo[0] = TextForUnitPanel;
				textinfo[1] = TextForUnitPanel1;
				textinfo[2] = TextForUnitPanel2;
				CreateDetail(textinfo, ref TMProTExt);
				SetCamera(TargetUnitCamera);
			}
		}

		private void CreateDetail(string[] details, ref TextMeshProUGUI[] TMProText)
		{
			if(_currentUnitDetails != null)
			{
				foreach(var item in _currentUnitDetails)
				{
					Destroy(item.gameObject);
				}
			}

			_currentUnitDetails = new TextMeshProUGUI[details.Length];
			for(int i = 0; i < details.Length; i++)
			{
				GameObject Tiles = (GameObject)Instantiate(_UnitDetailPrefab);
				Tiles.transform.SetParent(_UnitDetailPanel.transform);
				Tiles.GetComponent<TextMeshProUGUI>().text = details[i];
				_currentUnitDetails[i] = Tiles.GetComponent<TextMeshProUGUI>();
			}
			TMProText = _currentUnitDetails;
		}


		public void SetCamera(Transform TargetPosition)
		{
			_UnitCamera.position = TargetPosition.position + new Vector3(20, 10, 0);
			_UnitCamera.LookAt(TargetPosition);
		}

	}
}