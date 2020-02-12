using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace PPBA
{
	public class SyncNumberSlider : MonoBehaviour
	{
		[SerializeField] TextMeshProUGUI _text;
		public void Sync(float value)
		{
			_text.text = ((int)value).ToString();
		}
	}
}