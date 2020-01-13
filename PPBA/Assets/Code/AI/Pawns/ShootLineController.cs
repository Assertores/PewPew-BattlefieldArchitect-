using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class ShootLineController : MonoBehaviour
	{
		private LineRenderer _lineRenderer;
		private float _lineTicker = 0f;
		private float _lineDuration = 0.2f;

		#region Monobehaviour
		void Start()
		{

		}

		void Update()
		{
			if(_lineRenderer.enabled)
			{
				_lineTicker += Time.deltaTime;

				if(_lineDuration < _lineTicker)
					_lineRenderer.enabled = false;
			}
		}

		private void OnEnable()
		{
			if(null == _lineRenderer)
				_lineRenderer = GetComponent<LineRenderer>();
		}
		#endregion

		public void SetShootLine(Vector3 shooter, Vector3 target)
		{
			_lineRenderer.positionCount = 2;
			_lineRenderer.SetPositions(new Vector3[] { shooter, target });
			_lineTicker = 0f;
			_lineRenderer.enabled = true;
		}
	}
}