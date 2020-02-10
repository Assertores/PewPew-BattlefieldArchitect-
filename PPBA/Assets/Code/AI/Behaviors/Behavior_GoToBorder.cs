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
			if(pawn._borderData.z < 0.1f)
				pawn._currentAnimation = PawnAnimations.IDLE;
			else
				pawn._currentAnimation = PawnAnimations.RUN;

			//pawn.SetMoveTarget(new Vector3(pawn._borderData.x, pawn._borderData.y, pawn.transform.position.z));
			pawn.SetMoveTarget(new Vector3(pawn._borderData.x, pawn.transform.position.y, pawn._borderData.y));
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
#if DB_AI
					Debug.LogWarning("PawnAxisInputs defaulted to 1. Probably messed up the string name: " + name);
#endif
					return 1;
			}
		}

		protected float TargetAxisInputs(Pawn pawn, string name, Vector3 targetData)//add target
		{
			switch(name)
			{
				case "Distance":
				case "DistanceToTarget":
					//return Vector3.Distance(pawn.transform.position, target) / _maxDistance;
					return targetData.z / 100f;
				default:
#if DB_AI
					Debug.LogWarning("TargetAxisInputs defaulted to 1. Probably messed up the string name: " + name);
#endif
					return 1;
			}
		}

		public override float FindBestTarget(Pawn pawn) => CalculateTargetScore(pawn, pawn._borderData);

		protected float CalculateTargetScore(Pawn pawn, Vector3 targetData)
		{
			float score = 1;

			for(int i = 0; i < _targetAxes.Length; i++)
			{
				if(_targetAxes[i]._isEnabled)
					score *= Mathf.Clamp(_targetAxes[i]._curve.Evaluate(TargetAxisInputs(pawn, _targetAxes[i]._name, targetData)), 0f, 1f);
			}

			return score;
		}

		public override int GetTargetID(Pawn pawn) => pawn._id;

		public override void RemoveFromTargetDict(Pawn pawn)
		{
			if(s_targetDictionary.ContainsKey(pawn))
				s_targetDictionary.Remove(pawn);
		}
	}
}