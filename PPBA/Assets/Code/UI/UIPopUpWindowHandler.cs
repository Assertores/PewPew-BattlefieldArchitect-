using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class UIPopUpWindowHandler : Singleton<UIPopUpWindowHandler>
	{
		[SerializeField] GameObject _popUpPrefab;

		private void Start()
		{
			if(null == _popUpPrefab)
			{
				Debug.LogError("Pop Up Window Reference not set");
				Destroy(this);
			}
			if(null == _popUpPrefab.GetComponent<UIPopUpWindowRefHolder>())
			{
				Debug.LogError("Pop Up Window RefHolder not found");
				Destroy(this);
			}
			if(null == _popUpPrefab.GetComponent<UIPopUpWindowRefHolder>()._content)
			{
				Debug.LogError("Pop Up Window Content Reference not set");
				Destroy(this);
			}
		}

		public UIPopUpWindowRefHolder CreateWindow(string content)
		{
			UIPopUpWindowRefHolder value = Instantiate(_popUpPrefab, transform).GetComponent<UIPopUpWindowRefHolder>();
			value._content.text = content;
			return value;
		}
	}
}