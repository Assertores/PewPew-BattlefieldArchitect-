using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace PPBA
{
	public class Behavior_GoToFlag : Behavior
	{
		public static Behavior_GoToFlag s_instance;
		public static Dictionary<Pawn, FlagPole> s_targetDictionary = new Dictionary<Pawn, FlagPole>();
		protected const float _maxDistance = 100f;

		public Behavior_GoToFlag()
		{
			_name = Behaviors.GOTOFLAG;
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
			pawn._animationController.SetAnimatorBools(PawnAnimations.RUN);

			if(s_targetDictionary[pawn] != null && 0.1f < Vector3.Magnitude(s_targetDictionary[pawn].transform.position - pawn.transform.position))
			{
				pawn._currentAnimation = PawnAnimations.RUN;
				pawn.SetMoveTarget(s_targetDictionary[pawn].transform.position);
			}
			else
			{
				pawn._currentAnimation = PawnAnimations.IDLE;
				pawn.SetMoveTarget(pawn.transform.position);
			}
		}

		protected override float PawnAxisInputs(Pawn pawn, string name)
		{
			switch(name)
			{
				case StringCollection.HEALTH:
					return pawn._health / pawn._maxHealth;
				case StringCollection.AMMO:
					return (float) pawn._ammo / pawn._maxAmmo;
				case StringCollection.MORALE:
					return pawn._morale / pawn._maxMorale;
				default:
					Debug.LogWarning("PawnAxisInputs defaulted to 1. Probably messed up the string name: " + name);
					return 1;
			}
		}

		protected float TargetAxisInputs(Pawn pawn, FlagPole flagPole, string name)
		{
			switch(name)
			{
				case "Distance":
				case "DistanceToTarget":
					return Vector3.Distance(pawn.transform.position, flagPole.transform.position) / _maxDistance;
				case "Score":
					return flagPole._score;
				default:
					Debug.LogWarning("TargetAxisInputs defaulted to 1. Probably messed up the string name: " + name);
					return 1;
			}
		}
		
		protected float CalculateTargetScore(Pawn pawn, FlagPole flagPole)
		{
			float score = 1f;

			for(int i = 0; i < _targetAxes.Length; i++)
			{
				if(_targetAxes[i]._isEnabled)
				{
					score *= Mathf.Clamp(_targetAxes[i]._curve.Evaluate(TargetAxisInputs(pawn, flagPole, _targetAxes[i]._name)), 0f, 1f);
				}
			}

			return score;
		}

		public override float FindBestTarget(Pawn pawn)
		{
			float bestScore = 0f;

			if(null == JobCenter.s_flagPoles || null == JobCenter.s_flagPoles[pawn._team])
				return 0f;

			foreach(FlagPole flagPole in JobCenter.s_flagPoles[pawn._team])
			{
				float tempScore = CalculateTargetScore(pawn, flagPole);

				if(bestScore < tempScore)
				{
					s_targetDictionary[pawn] = flagPole;
					bestScore = tempScore;
				}
			}

			return bestScore;
		}

		public override int GetTargetID(Pawn pawn)
		{
			if(s_targetDictionary.ContainsKey(pawn) && null != s_targetDictionary[pawn])
				return s_targetDictionary[pawn]._id;
			else
				return -1;
		}

		public override void RemoveFromTargetDict(Pawn pawn)
		{
			if(s_targetDictionary.ContainsKey(pawn))
				s_targetDictionary.Remove(pawn);
		}
	}
}