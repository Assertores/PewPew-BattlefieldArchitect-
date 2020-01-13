using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

namespace PPBA
{
	public class UIPopUpWindowRefHolder : MonoBehaviour, IDragHandler, IBeginDragHandler
	{
		public TextMeshProUGUI _content;

		private Vector3 _offset;

		public void CloseWindow()
		{
			Destroy(this.gameObject);
		}

		public void Disconnect()
		{
			GameNetcode.s_instance.ClientDisconnect();
		}

		public void OnDrag(PointerEventData eventData)
		{
			((RectTransform)transform).position = Input.mousePosition + _offset;
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
			_offset = new Vector3(((RectTransform)transform).position.x, ((RectTransform)transform).position.y) - Input.mousePosition;
		}
	}
}