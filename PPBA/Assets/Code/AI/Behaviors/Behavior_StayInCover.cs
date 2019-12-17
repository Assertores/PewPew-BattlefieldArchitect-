using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class Behavior_StayInCover : Behavior
	{
		public static Behavior_StayInCover s_instance;
		public static Dictionary<Pawn, Pawn> s_targetDictionary = new Dictionary<Pawn, Pawn>();

		public Behavior_StayInCover()
		{
			_name = Behaviors.STAYINCOVER;
		}

		#region Monobehaviour
		private void Awake()
		{
			if(s_instance == null)
				s_instance = this;
			else
				Destroy(gameObject);
		}
		#endregion

		public override void Execute(Pawn pawn) => Behavior_Shoot.s_instance.Execute(pawn);

		public override float FindBestTarget(Pawn pawn) => Behavior_Shoot.s_instance.FindBestTarget(pawn);

		protected float CalculateTargetScore(Pawn pawn, Pawn target) => Behavior_Shoot.s_instance.CalculateTargetScore(pawn, target);

		public override int GetTargetID(Pawn pawn) => Behavior_Shoot.s_instance.GetTargetID(pawn);

		protected override float PawnAxisInputs(Pawn pawn, string name)
		{
			switch(name)
			{
				case StringCollection.HEALTH:
					return pawn._health / pawn._maxHealth;
				case StringCollection.COVER:
					return pawn._isMounting ? pawn._mountSlot._coverScore : 0f;
				case "IsMounting":
					return pawn._isMounting ? 1f : 0f;
				default:
					return 1f;
			}
		}

		protected float TargetAxisInputs(Pawn pawn, string name, Pawn target)
		{
			switch(name)
			{
				/*
				case "Distance":
					return Vector3.Distance(pawn.transform.position, mountSlot.transform.position) / _maxDistance;
				case "Score":
					return mountSlot._coverScore;
					*/
				default:
					return 1f;
			}
		}

		public override void RemoveFromTargetDict(Pawn pawn)
		{
			if(s_targetDictionary.ContainsKey(pawn))
			{
				s_targetDictionary.Remove(pawn);
			}

			pawn._mountSlot?.GetOut(pawn);
		}

		public bool EvaluateCover(Pawn pawn)
		{


			return true;
		}
	}
}