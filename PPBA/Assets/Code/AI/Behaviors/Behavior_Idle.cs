﻿using System.Collections;
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

		void Start()
		{

		}

		void Update()
		{

		}

		public override void Execute(Pawn pawn) => pawn.SetMoveTarget(pawn.transform.position);

		public override float FindBestTarget(Pawn pawn) => 1f;

		protected override float PawnAxisInputs(Pawn pawn, string name)
		{
			switch(name)
			{
				case "Health":
					return pawn._health / pawn._maxHealth;
				default:
					Debug.LogWarning("PawnAxisInputs defaulted to 1. Probably messed up the string name: " + name);
					return 1f;
			}
		}

		protected float TargetAxisInputs(Pawn pawn, string name) => 1f;

		protected float CalculateTargetScore(Pawn pawn) => 1f;

		public override int GetTargetID(Pawn pawn) => pawn._id;

		public override void RemoveFromTargetDict(Pawn pawn)
		{
			//empty because no target dictionary
		}
	}
}