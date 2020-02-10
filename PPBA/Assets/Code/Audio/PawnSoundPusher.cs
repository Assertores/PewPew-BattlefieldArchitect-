using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class PawnSoundPusher : MonoBehaviour
	{
		private PawnBoomboxController _boombox;

		private void Awake()
		{
			_boombox = transform.parent.GetComponentInChildren<PawnBoomboxController>();
		}

		private void Step() => _boombox.PlayStep();

		//private void Shoot() => _boombox.
	}
}