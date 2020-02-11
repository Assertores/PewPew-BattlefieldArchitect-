using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class ShootLineController : MonoBehaviour
	{
		private LineRenderer _lineRenderer;
		private float _lineTicker = 0f;
		[SerializeField] private float _lineDuration = 0.2f;
		private PawnBoomboxController _boombox;

		#region Monobehaviour
		void Start()
		{
			_boombox = transform.parent.GetComponentInChildren<PawnBoomboxController>();
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

			if(null == _boombox)
				_boombox = transform.parent.GetComponentInChildren<PawnBoomboxController>();
		}
		#endregion

		public void SetShootLine(Vector3 shooter, Vector3 target)
		{
			_lineRenderer.positionCount = 2;
			//_lineRenderer.SetPositions(new Vector3[] { shooter, target });
			_lineRenderer.SetPositions(new Vector3[] { shooter, new Vector3(target.x, target.y + 0.5f, target.z) });
			_lineTicker = 0f;
			_lineRenderer.enabled = true;

			_boombox.PlayBehavior(ClipsPawn.SHOT_MP_01);
		}
	}
}