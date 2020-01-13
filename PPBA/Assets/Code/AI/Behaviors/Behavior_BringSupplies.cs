using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class Behavior_BringSupplies : Behavior
	{
		public static Behavior_BringSupplies s_instance;
		public static Dictionary<Pawn, Blueprint> s_targetDictionary = new Dictionary<Pawn, Blueprint>();

		[SerializeField] [Tooltip("How many resources does a pawn grab at once?")] private int _grabSize = 10;

		public Behavior_BringSupplies()
		{
			_name = Behaviors.BRINGSUPPLIES;
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
					pawn._currentAnimation = PawnAnimations.IDLE;
					s_targetDictionary[pawn].GiveResources(Mathf.Min(_grabSize, pawn._supplies));
					targetPosition = pawn.transform.position;
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
				case StringCollection.HEALTH:
					return pawn._health / pawn._maxHealth;
				case StringCollection.SUPPLIES:
					return (float) pawn._supplies / pawn._maxSupplies;
				default:
					Debug.LogWarning("PawnAxisInputs defaulted to 1. Probably messed up the string name: " + name);
					return 1;
			}
		}

		protected float TargetAxisInputs(Pawn pawn, string name, Blueprint blueprint)
		{
			switch(name)
			{
				case "Distance":
					return Vector3.Distance(pawn.transform.position, blueprint.transform.position) / 40f;
				case "SuppliesNeeded":
					return blueprint._resourcesNeeded;//NOT 0..1 YET !!
				/*
				case "Score":
				return blueprint._score;
				*/
				default:
					Debug.LogWarning("TargetAxisInputs defaulted to 1. Probably messed up the string name: " + name);
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

		public void CalculateIncomingSupplies(int tick = 0)//not called yet
		{
			foreach(Pawn pawn in Pawn._pawns.FindAll((x) => x._lastBehavior == Behavior_BringSupplies.s_instance))
			{
				if(s_targetDictionary.ContainsKey(pawn) && null != s_targetDictionary[pawn])
					s_targetDictionary[pawn]._resourcesIncoming += pawn._supplies;
			}
		}

		public void PromiseSupplies(Blueprint blueprint, int value)//not called yet
		{
			blueprint._resourcesIncoming += value;
		}
	}
}