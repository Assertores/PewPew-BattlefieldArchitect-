using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class Behavior_Idle : Behavior
	{
		public static Behavior_Idle s_instance;

		private void Awake()
		{
			if(s_instance == null)
				s_instance = this;
			else
				Destroy(gameObject);
		}

		// Start is called before the first frame update
		void Start()
		{

		}

		// Update is called once per frame
		void Update()
		{

		}

		public override void Execute(Pawn pawn)
		{
			pawn.SetMoveTarget(pawn.transform.position);
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
			throw new System.NotImplementedException();
		}
	}
}