using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class Behavior_Die : Behavior
	{
		public static Behavior_Die s_instance;

		private void Awake()
		{
			if(s_instance == null)
				s_instance = this;
			else
				Destroy(gameObject);
		}

		void Start()
		{

		}

		void Update()
		{

		}

		public override void Execute(Pawn pawn)
		{
			pawn.SetMoveTarget(pawn.transform.position);

			foreach(Pawn p in pawn._activePawns)
			{
				if(pawn._team == p._team)
					p._morale += Moralizer.s_instance.PassJudgement(MoraleEvents.CLOSEPAWNDIED);
				else
					p._morale += Moralizer.s_instance.PassJudgement(MoraleEvents.ENEMYDIED);
			}

			//put pawn back into object pool
			pawn.gameObject.SetActive(false);
		}

		public override float FindBestTarget(Pawn pawn)
		{
			return 1;
		}

		protected override float PawnAxisInputs(Pawn pawn, string name)
		{
			switch(name)
			{
				case "Health":
					return pawn._health / pawn._maxHealth;
				default:
					Debug.LogWarning("PawnAxisInputs defaulted to 1. Probably messed up the string name: " + name);
					return 1;
			}
		}

		protected float TargetAxisInputs(Pawn pawn, string name, ResourceDepot depot)
		{
			return 1;
		}

		protected float CalculateTargetScore(Pawn pawn, ResourceDepot depot)
		{
			return 1;
		}

		public override int GetTargetID(Pawn pawn)
		{
			return pawn._id;
		}

		public override void RemoveFromTargetDict(Pawn pawn)
		{
			//empty because no target dictionary
		}
	}
}