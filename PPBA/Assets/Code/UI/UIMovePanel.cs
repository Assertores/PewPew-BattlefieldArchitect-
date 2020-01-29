using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class UIMovePanel : MonoBehaviour
	{
		[SerializeField] float _speed = 0.25f;
		[SerializeField] RectTransform _panle;

		float _startPanlePos;
		bool isPanelOut = false;

		private void Awake()
		{
			_startPanlePos = _panle.anchoredPosition.y;
		}
		public void MovePanel()
		{
			StopAllCoroutines();
			isPanelOut = !isPanelOut;
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
