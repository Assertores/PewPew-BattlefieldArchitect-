using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class Behavior_Build : Behavior
	{
		public static Behavior_Build s_instance;
		public static Dictionary<Pawn, Blueprint> s_targetDictionary = new Dictionary<Pawn, Blueprint>();

		public Behavior_Build()
		{
			_name = Behaviors.BUILD;
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

		public override void Execute(Pawn pawn)
		{
			if(s_targetDictionary.ContainsKey(pawn) && null != s_targetDictionary[pawn] && s_targetDictionary[pawn].isActiveAndEnabled)
			{
				Vector3 targetPosition = s_targetDictionary[pawn].transform.position;

				if(Vector3.Magnitude(targetPosition - pawn.transform.position) < s_targetDictionary[pawn]._interactRadius)
				{
					s_targetDictionary[pawn].DoWork();
					targetPosition = pawn.transform.position;
					pawn._currentAnimation = PawnAnimations.BUILD;
					pawn._arguments |= Arguments.TRIGGERBEHAVIOUR;//set trigger flag for build sound
				}
				else
					pawn._currentAnimation = PawnAnimations.RUN;

				pawn.SetMoveTarget(targetPosition);
			}
		}

		public override float FindBestTarget(Pawn pawn)
		{
			float bestScore = 0f;

			foreach(Blueprint blueprint in JobCenter.s_blueprints[pawn._team])
			{
				float tempScore = CalculateTargetScore(pawn, blueprint);

				if(bestScore < tempScore)
				{
					s_targetDictionary[pawn] = blueprint;
					bestScore = tempScore;
				}
			}

			return bestScore;
		}

		protected override float PawnAxisInputs(Pawn pawn, string name)
		{
			switch(name)
			{
				case "Health":
					return pawn._health / pawn._maxHealth;
				default:
#if DB_AI
					Debug.LogWarning("PawnAxisInputs defaulted to 1. Probably messed up the string name: " + name);
#endif
					return 1;
			}
		}

		protected float TargetAxisInputs(Pawn pawn, string name, Blueprint blueprint)
		{
			switch(name)
			{
				case "Distance":
					return Vector3.Distance(pawn.transform.position, blueprint.transform.position) / 60f;
				case "WorkDoable":
					return blueprint._workDoable;
				default:
#if DB_AI
					Debug.LogWarning("TargetAxisInputs defaulted to 1. Probably messed up the string name: " + name);
#endif
					return 1;
			}
		}

		protected float CalculateTargetScore(Pawn pawn, Blueprint blueprint)
		{
			float _score = 1;

			for(int i = 0; i < _targetAxes.Length; i++)
			{
				if(_targetAxes[i]._isEnabled)
					_score *= Mathf.Clamp(_targetAxes[i]._curve.Evaluate(TargetAxisInputs(pawn, _targetAxes[i]._name, blueprint)), 0f, 1f);
			}

			return _score;
		}

		public override int GetTargetID(Pawn pawn)
		{
			if(s_targetDictionary.ContainsKey(pawn))
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