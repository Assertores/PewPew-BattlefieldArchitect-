using UnityEngine;
using DG.Tweening;
using TMPro;

namespace PPBA
{
	public class UnitScreenController : Singleton<UnitScreenController>
	{
		public IPanelInfo _activePanel;
		public RectTransform _unitPanel;
		public Transform _unitDetailPanel;

		public GameObject _unitDetailPrefab;
		public Transform _unitCamera;
		//private bool _isUnitPanel = false;
		private bool _isUnitPanel => _activePanel != null;
		private TextMeshProUGUI[] _currentUnitDetails;

		private void Update()
		{
#if !UNITY_SERVER
			if(Input.GetMouseButtonDown(0))
			{
				IPanelInfo unit = GetClickedObject();//TODO: get the object

				if(unit == null && _isUnitPanel)
					CloseUnitPanel();

				SetActivePanel(unit);
			}

			if(Input.GetMouseButton(1))
				CloseUnitPanel();
#endif
		}

		private void LateUpdate()
		{
#if !UNITY_SERVER
			//refresh panel info
			if(_activePanel != null)
				_activePanel.UpdateUnitPanelInfo();
#endif
		}

		public void SetActivePanel(IPanelInfo unit)//call on mouse click
		{
			if(_activePanel == unit)//fall: ist gleiches ziel
			{
				return;
			}
			else if(!_isUnitPanel)//fall: neues ziel, bisher kein panel
			{
				_activePanel = unit;
				OpenUnitPanel();
				unit.InitialiseUnitPanel();
			}
			else if(_isUnitPanel)//fall: hat schon ziel
			{
				_activePanel = unit;
				unit.InitialiseUnitPanel();//atm also deletes old details
			}
		}

		public void TogglePanel()
		{
			if(_isUnitPanel)
				CloseUnitPanel();
			else
				OpenUnitPanel();
		}

		public void OpenUnitPanel()
		{
			//opens
			_unitPanel.DOAnchorPos(new Vector2(-_unitPanel.rect.x, 400), 0.25f);
			_activePanel.InitialiseUnitPanel();
		}

		private void CloseUnitPanel()
		{
			//closes
			_unitPanel.DOAnchorPos(new Vector2(-_unitPanel.rect.x, 0), 0.25f);
			_activePanel = null;
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
				GameObject Tiles = (GameObject)Instantiate(_unitDetailPrefab);
				Tiles.transform.SetParent(_unitDetailPanel.transform);
				Tiles.GetComponent<TextMeshProUGUI>().text = details[i];
				_currentUnitDetails[i] = Tiles.GetComponent<TextMeshProUGUI>();
			}
			TMProText = _currentUnitDetails;
		}

		public void SetCamera(Transform TargetPosition)
		{
			_unitCamera.position = TargetPosition.position + new Vector3(20, 10, 0);
			_unitCamera.LookAt(TargetPosition);
		}

		[SerializeField] [Tooltip("Objects on which layers have panels?")] private LayerMask _layerMask;
		public IPanelInfo GetClickedObject()
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			RaycastHit hitInfo;
			if(Physics.Raycast(ray, out hitInfo, 1000, _layerMask))
				return hitInfo.transform.GetComponent<IPanelInfo>();
			else
				return null;
		}
	}
}