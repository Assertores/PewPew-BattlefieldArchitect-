using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PPBA
{
	public class UIToolTip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		[SerializeField] GameObject _toolTip;

		private void Start()
		{
#if UNITY_EDITOR
			if(!_toolTip)
			{
				Debug.LogError("Reference to ToolTip was not set");
				Destroy(this);
				return;
			}
#endif

			_toolTip.SetActive(false);
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			_toolTip.SetActive(true);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			_toolTip.SetActive(false);
		}
	}
}
