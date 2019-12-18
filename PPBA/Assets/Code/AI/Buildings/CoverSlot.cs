using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class CoverSlot : MountSlot
	{
		//public
		public Cover _parentCover;

		void Awake()
		{
			
		}

		void Start()
		{

		}

		void Update()
		{

		}

		#region Pawn Interaction
		public override void GetHit(int amount) => _parentCover?.TakeDamage(amount);
		#endregion

		#region Tick
		public override void CalculateScore(int tick = 0)
		{
			//how close to border
			//how many enemy pawns

			/*
			if(null != _parentCover)
				return 1f;
			else
				return 0f;
				*/
		}

		public override float GetCoverScore(Vector3 shooterPosition)
		{
			return _coverScore;

			//	if(null != _parentCover && Vector3.Magnitude(_parentCover.transform.position - shooterPosition) < Vector3.Magnitude(transform.position - shooterPosition))//Is CoverPoint further from shooter than _parentCover?
			//		return _coverScore;
			//	else
			//		return 0f;
		}

		public override void Execute()
		{
			//shoot
		}

		public override void WriteToGameState(int tick)
		{
			base.WriteToGameState(tick);
		}
		#endregion
	}
}