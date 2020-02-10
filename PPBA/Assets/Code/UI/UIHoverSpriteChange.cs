using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PPBA
{
	public class UIHoverSpriteChange : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		[SerializeField] Image _image;
		[SerializeField] Sprite _normal;
		[SerializeField] Sprite _highlight;

		private void Awake()
		{
#if UNITY_EDITOR
			if(!_image)
			{
				Debug.Log("image reference not set");
				Destroy(this);
				return;
			}
			if(!_normal)
			{
				Debug.Log("no normal image");
				Destroy(this);
				return;
			}
			if(!_highlight)
			{
				Debug.Log("no highlited image");
				Destroy(this);
				return;
			}
#endif
			_image.sprite = _normal;
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			_image.sprite = _highlight;
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			_image.sprite = _normal;
		}
	}
}
