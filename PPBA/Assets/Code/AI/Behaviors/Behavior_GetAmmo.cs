using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class Behavior_GetAmmo : Behavior
	{
		public static Behavior_GetAmmo s_instance;
		public static Dictionary<Pawn, ResourceDepot> s_targetDictionary = new Dictionary<Pawn, ResourceDepot>();

		[SerializeField] [Tooltip("How many resources does a pawn grab at once?")] private int _grabSize = 10;

		public Behavior_GetAmmo()
		{
			_name = Behaviors.GETAMMO;
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
			pawn._currentAnimation = PawnAnimations.RUN;

			Vector3 targetPosition = s_targetDictionary[pawn].transform.position;
			pawn.SetMoveTarget(targetPosition);

			if(Vector3.Magnitude(targetPosition - pawn.transform.position) < s_targetDictionary[pawn]._interactRadius)
			{
				//Takes an amount of ammo from the depot no larger than (1) the space left at pawn (2) the ammo left at depot, and gives it to the pawn.
				pawn._ammo += s_targetDictionary[pawn].TakeAmmo(Mathf.Min(_grabSize, pawn._maxAmmo - pawn._ammo));
			}
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
				case "Ammo":
					return (float) pawn._ammo / pawn._maxAmmo;
				default:
					Debug.LogWarning("PawnAxisInputs defaulted to 1. Probably messed up the string name: " + name);
					return 1;
			}
		}

		protected float TargetAxisInputs(Pawn pawn, string name, ResourceDepot depot)
		{
			switch(name)
			{
				case "Distance":
					return Vector3.Distance(pawn.transform.position, depot.transform.position) / 60f;
				//case "Score":
				//	return depot._score;
				case "Ammo":
					return (float) depot._ammo / depot._maxAmmo;
				default:
					Debug.LogWarning("TargetAxisInputs defaulted to 1. Probably messed up the string name: " + name);
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