using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace PPBA {
	public class UIServerMenu : MonoBehaviour
	{
		[SerializeField] TMP_InputField _portField;
		[SerializeField] TMP_InputField _playerCountField;

		void Start()
		{
#if !UNITY_SERVER
			Destroy(this.gameObject);
			return;
#endif
			if(!_portField)
			{
				Debug.LogError("portField reference not set");
				Destroy(this);
				return;
			}
			if(!_playerCountField)
			{
				Debug.LogError("player count reference not set");
				Destroy(this);
				return;
			}
		}

		public void StartServer()
		{
			Debug.Log("Change Scene");
			int port = _portField.text == "" ? 13370 : int.Parse(_portField.text);
			int count = _playerCountField.text == "" ? 2 : int.Parse(_playerCountField.text);
			GameNetcode.s_instance.ServerStart(port, count);
		}
	}
}