using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace PPBA
{
	public class UIMainMenuHandler : MonoBehaviour
	{

		#region Variables

		[SerializeField] TMP_InputField _ipField;
		[SerializeField] TMP_InputField _portField;
		[SerializeField] GameObject _startPannel;
		[SerializeField] GameObject _optionsPannel;
		[SerializeField] GameObject _controlsPannel;
		[SerializeField] GameObject _creditsPannel;

		#endregion

		private void Start()
		{
			if(!_ipField)
			{
				Debug.LogError("ipField reference not set");
				Destroy(this);
				return;
			}
			if(!_portField)
			{
				Debug.LogError("portField reference not set");
				Destroy(this);
				return;
			}
			if(!_startPannel)
			{
				Debug.LogError("startPannel reference not set");
				Destroy(this);
				return;
			}
			if(!_optionsPannel)
			{
				Debug.LogError("optionsPannel reference not set");
				Destroy(this);
				return;
			}
			if(!_controlsPannel)
			{
				Debug.LogError("controlsPannel reference not set");
				Destroy(this);
				return;
			}
			if(!_creditsPannel)
			{
				Debug.LogError("creditsPannel reference not set");
				Destroy(this);
				return;
			}

			_startPannel.SetActive(false);
			_optionsPannel.SetActive(false);
			_controlsPannel.SetActive(false);
			_creditsPannel.SetActive(false);
		}

		public void ToggleStartPannel()
		{
			_startPannel.SetActive(!_startPannel.activeSelf);
			_optionsPannel.SetActive(false);
			_controlsPannel.SetActive(false);
			_creditsPannel.SetActive(false);
		}

		public void ToggleOptionsPannel()
		{
			_startPannel.SetActive(false);
			_optionsPannel.SetActive(!_optionsPannel.activeSelf);
			_controlsPannel.SetActive(false);
			_creditsPannel.SetActive(false);
		}

		public void ToggleControlsPannel()
		{
			_startPannel.SetActive(false);
			_optionsPannel.SetActive(false);
			_controlsPannel.SetActive(!_controlsPannel.activeSelf);
			_creditsPannel.SetActive(false);
		}

		public void ToggleCreditsPannel()
		{
			_startPannel.SetActive(false);
			_optionsPannel.SetActive(false);
			_controlsPannel.SetActive(false);
			_creditsPannel.SetActive(!_creditsPannel.activeSelf);
		}

		public void ConnectToServer()
		{
			string ip = _ipField.text == "" ? _ipField.placeholder : _ipField.text;
			Debug.Log(_ipField.);
			//GameNetcode.s_instance.ClientConnect(_ipField.text, _);
		}

		public void Quit()
		{
			Application.Quit();
#if UNITY_EDITOR
			Debug.Break();
#endif
		}
	}
}
