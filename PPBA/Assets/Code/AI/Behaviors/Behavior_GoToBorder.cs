using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace PPBA
{
	public class Behavior_GoToBorder : Behavior
	{
		public static Behavior_GoToBorder s_instance;
		public static Dictionary<Pawn, Vector3> s_targetDictionary = new Dictionary<Pawn, Vector3>();
		protected const float _maxDistance = 100f;

		public Behavior_GoToBorder()
		{
			_name = Behaviors.GOTOBORDER;
		}

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
			pawn._currentAnimation = PawnAnimations.RUN;
		}

		protected override float PawnAxisInputs(Pawn pawn, string name)
		{
			switch(name)
			{
				case "Health":
					return pawn._health / pawn._maxHealth;
				case "Ammo":
					return pawn._ammo / pawn._maxAmmo;
				case "Morale":
					return pawn._morale / pawn._maxMorale;
				default:
					Debug.LogWarning("PawnAxisInputs defaulted to 1. Probably messed up the string name: " + name);
					return 1;
			}
		}

		protected float TargetAxisInputs(Pawn pawn, Vector3 target, string name)//add target
		{
			switch(name)
			{
				case "Distance":
				case "DistanceToTarget":
					return Vector3.Distance(pawn.transform.position, target) / _maxDistance;
				default:
					Debug.LogWarning("TargetAxisInputs defaulted to 1. Probably messed up the string name: " + name);
					return 1;
			}
		}

		public override float FindBestTarget(Pawn pawn)
		{
			//use some awesome map to find closest border to the pawn
			
			return 1;
		}

		public override int GetTargetID(Pawn pawn)
		{
			throw new System.NotImplementedException();
		}

		public override void RemoveFromTargetDict(Pawn pawn)
		{
			if(s_targetDictionary.ContainsKey(pawn))
				s_targetDictionary.Remove(pawn);
		}
	}
}