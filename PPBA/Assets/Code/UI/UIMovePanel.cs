using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PPBA
{
	public class UIMovePanel : MonoBehaviour
	{
		[SerializeField] float _speed = 0.25f;
		[SerializeField] RectTransform _panle;
		[SerializeField] Image _icon;
		[SerializeField] Sprite _open;
		[SerializeField] Sprite _close;

		float _startPanlePos;
		bool isPanelOut = false;

		private void Awake()
		{
			_startPanlePos = _panle.anchoredPosition.y;
			_icon.sprite = _open;
		}
		public void MovePanel()
		{
			StopAllCoroutines();
			isPanelOut = !isPanelOut;
			_icon.sprite = isPanelOut ? _close : _open;
			StartCoroutine(IEMovePanel());
		}

		IEnumerator IEMovePanel()
		{
			float startTime = Time.time;
			float startPos = _panle.anchoredPosition.y;

			while(Time.time - startTime < _speed)
			{
				_panle.anchoredPosition = new Vector2(_panle.anchoredPosition.x, Mathf.Lerp(startPos, isPanelOut ? 0 : _startPanlePos, (Time.time - startTime) / _speed));
				yield return null;
			}

			_panle.anchoredPosition = new Vector2(_panle.anchoredPosition.x, isPanelOut ? 0 : _startPanlePos);
		}
	}
}
