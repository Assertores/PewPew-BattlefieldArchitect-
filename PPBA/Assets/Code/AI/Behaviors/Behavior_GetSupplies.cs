﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class Behavior_GetSupplies : Behavior
	{
		public static Behavior_GetSupplies s_instance;
		public static Dictionary<Pawn, ResourceDepot> s_targetDictionary = new Dictionary<Pawn, ResourceDepot>();

		//[SerializeField] [Tooltip("How many resources does a pawn grab at once?")] private int _grabSize = 10;

		public Behavior_GetSupplies()
		{
			_name = Behaviors.GETSUPPLIES;
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
			Vector3 targetPosition = s_targetDictionary[pawn].transform.position;

			if(Vector3.Magnitude(targetPosition - pawn.transform.position) < s_targetDictionary[pawn]._interactRadius)
			{
				pawn._currentAnimation = PawnAnimations.IDLE;
				//Takes an amount of resources from the depot no larger than (1) the space left at pawn (2) the res left at depot, and gives it to the pawn.
				pawn._supplies += s_targetDictionary[pawn].TakeResources(Mathf.Min(pawn._maxSupplies, pawn._maxSupplies - pawn._supplies));
				targetPosition = pawn.transform.position;
			}
			else
				pawn._currentAnimation = PawnAnimations.RUN;

			pawn.SetMoveTarget(targetPosition);
		}

		public override float FindBestTarget(Pawn pawn)
		{
			float bestScore = 0f;

			foreach(ResourceDepot depot in JobCenter.s_resourceDepots[pawn._team])
			{
				float tempScore = CalculateTargetScore(pawn, depot);

				if(bestScore < tempScore)
				{
					s_targetDictionary[pawn] = depot;
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
				case "Resources":
					return (float) pawn._supplies / pawn._maxSupplies;
				default:
#if DB_AI
					Debug.LogWarning("PawnAxisInputs defaulted to 1. Probably messed up the string name: " + name);
#endif
					return 1;
			}
		}

		protected float TargetAxisInputs(Pawn pawn, string name, ResourceDepot depot)
		{
			switch(name)
			{
				case "Distance":
					return Vector3.Distance(pawn.transform.position, depot.transform.position) / 60f;
				case "Score":
					return depot._score;
				case "Resources":
					return (float) depot._resources / depot._maxResources;
				default:
#if DB_AI
					Debug.LogWarning("TargetAxisInputs defaulted to 1. Probably messed up the string name: " + name);
#endif
					return 1;
			}
		}

		protected float CalculateTargetScore(Pawn pawn, ResourceDepot depot)
		{
			float _score = 1;

			for(int i = 0; i < _targetAxes.Length; i++)
			{
				if(_targetAxes[i]._isEnabled)
					_score *= Mathf.Clamp(_targetAxes[i]._curve.Evaluate(TargetAxisInputs(pawn, _targetAxes[i]._name, depot)), 0f, 1f);
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