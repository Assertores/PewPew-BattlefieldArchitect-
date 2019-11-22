using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class Behavior_Shoot : Behavior
	{
		public static Behavior_Shoot s_instance;
		public static Dictionary<Pawn, Pawn> s_targetDictionary;

		void Awake()//my own singleton pattern, the Singleton.cs doesn't work here as I need multiple behaviors.
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

		}

		public override float FindBestTarget(Pawn pawn)
		{
			float bestScore = 0;

			foreach(Pawn target in pawn._activePawns.FindAll(x => x._team != pawn._team))
			{
				float tempScore = CalculateTargetScore(pawn, target);

				if(bestScore < tempScore)
				{
					s_targetDictionary[pawn] = target;
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
					return pawn._ammo / pawn._maxAmmo;
				case "Cover":
					return 0.5f;//return actual cover value
				case "Morale":
					return pawn._morale / pawn._maxMorale;
				default:
					Debug.LogWarning("PawnAxisInputs defaulted to 1. Probably messed up the string name: " + name);
					return 1;
			}
		}

		protected float TargetAxisInputs(Pawn pawn, string name, Pawn target)
		{
			switch(name)
			{
				case "Distance":
					return Vector3.Distance(target.transform.position, pawn.transform.position) / pawn._attackDistance;
				case "Health":
					return target._health / target._maxHealth;
				case "Cover":
					return 1;//got to implement a cover system
				case "DistanceToMyBase":
					//return Vector3.Distance(s_targetDictionary[pawn].transform.position, HQ.TRANSFORM.POSITION) / 100f;
					return 1;
				case "ShotOnMe":
					return s_targetDictionary[target] == pawn ? 1f : 0f;
				default:
					break;
			}

			return 1;
		}

		protected float CalculateTargetScore(Pawn pawn, Pawn target)
		{
			if(!CheckLos(pawn, target))//early skip when LOS is blocked
				return 0;

			float score = 1f;

			for(int i = 0; i < _targetAxes.Length; i++)
			{
				if(_targetAxes[i]._isEnabled)
				{
					score *= Mathf.Clamp(_targetAxes[i]._curve.Evaluate(TargetAxisInputs(pawn, _targetAxes[i]._name, target)), 0f, 1f);
				}
			}

			return score;
		}

		private bool CheckLos(Pawn pawn, Pawn target)
		{
			//check for wall with ray/linecast (+layerMask)

			return true;
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

		public void Shoot(Pawn pawn, Pawn target)
		{
			if(0 < pawn._ammo)//skip if no ammo
				pawn._ammo--;
			else
				return;

			if(Random.Range(0f, 1f) < pawn._attackChance)//roll to hit anything
			{
				if(target._isMounting)
				{
					if(Random.Range(0f, 1f) < target._mountSlot._coverScore)//roll to hit cover
					{
						return;
					}
				}

				target.TakeDamage(RollDamage(pawn));//target hit succesfully
			}
		}

		private int RollDamage(Pawn pawn) => (int)Mathf.Lerp(pawn._minAttackDamage, pawn._maxAttackDamage, Mathf.Clamp(pawn._attackDamageCurve.Evaluate(Random.Range(0f, 1f)), 0f, 1f));
	}
}