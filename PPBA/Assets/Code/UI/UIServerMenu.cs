using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using System.Net;

namespace PPBA {
	public class UIServerMenu : MonoBehaviour
	{
		[SerializeField] TextMeshProUGUI _ipField;
		[SerializeField] TMP_InputField _portField;
		[SerializeField] Slider _playerCount;
		[SerializeField] Slider _botLimit;
		[SerializeField] Slider _hmRes;//heatmap Resolution
		bool _regToMaster;//register to Master server
		[SerializeField] TMP_Dropdown _map;

		void Start()
		{
#if !UNITY_SERVER
			Destroy(this.gameObject);
			return;
#endif
			if(!_ipField)
			{
				Debug.LogError("IPField reference not set");
				Destroy(this);
				return;
			}
			if(!_portField)
			{
				Debug.LogError("portField reference not set");
				Destroy(this);
				return;
			}
			if(!_playerCount)
			{
				Debug.LogError("player count reference not set");
				Destroy(this);
				return;
			}
			if(!_botLimit)
			{
				Debug.LogError("bot limit reference not set");
				Destroy(this);
				return;
			}
			if(!_hmRes)
			{
				Debug.LogError("heatmap Resolution reference not set");
				Destroy(this);
				return;
			}
			if(!_map)
			{
				Debug.LogError("map dropdown reference not set");
				Destroy(this);
				return;
			}

			string hostName = Dns.GetHostName(); // Retrive the Name of HOST
			Debug.Log(hostName);
			string myIP = "";
			foreach(var it in Dns.GetHostEntry(hostName).AddressList)// get IPv4
			{
				if(it.AddressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6)
				{
					myIP = it.ToString();
					break;
				}
			}
			_ipField.text = myIP;
		}

		public void StartServer()
		{
			Debug.Log("Change Scene");
			int port = _portField.text == "" ? 13370 : int.Parse(_portField.text);
			MainMenuToGame.s_instance.ServerExecute(port, (int)_playerCount.value, _map.value, (int)_botLimit.value, GetHMRes(),_regToMaster);
		}

		public void SetMasterServer(bool set)
		{
			_regToMaster = set;
		}

		int GetHMRes()
		{
			return 1 << (int)_hmRes.value;
		}
	}
}