using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class Behavior_Shoot : Behavior
	{
		public static Behavior_Shoot instance;
		public static Dictionary<Pawn, Pawn> s_targetDictionary;

		void Awake()//my own singleton pattern, the Singleton.cs doesn't work here as I need multiple behaviors.
		{
			if(instance == null)
				instance = this;
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
			throw new System.NotImplementedException();
		}

		public override float FindBestTarget(Pawn pawn)
		{
			throw new System.NotImplementedException();
		}

		protected override float PawnAxisInputs(Pawn pawn, string name)
		{
			switch(name)
			{
				case "Health":
					return pawn.health / pawn.maxHealth;
				case "Ammo":
					return pawn.ammo / pawn.maxAmmo;
				case "Cover":
					return 0.5f;//return actual cover value
				case "Morale":
					return pawn.morale / pawn.maxMorale;
				default:
					Debug.LogWarning("PawnAxisInputs defaulted to 1. Probably messed up the string name: " + name);
					return 1;
			}
		}

		protected float TargetAxisInputs(Pawn pawn, string name)
		{
			return 1;
		}

		private bool CheckLos(Pawn pawn, Pawn target)
		{
			//check for wall with ray/linecast

			return true;
		}
	}
}