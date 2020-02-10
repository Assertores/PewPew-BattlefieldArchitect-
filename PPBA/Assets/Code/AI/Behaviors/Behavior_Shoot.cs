using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class Behavior_Shoot : Behavior
	{
		//public
		public static Behavior_Shoot s_instance;
		public static Dictionary<Pawn, Pawn> s_targetDictionary = new Dictionary<Pawn, Pawn>();
		public static Dictionary<Pawn, int> s_timerDictionary = new Dictionary<Pawn, int>();

		//private
		[SerializeField] [Tooltip("How long from starting the attack to shooting in ticks?")] private int _attackBuildUpTime = 8;
		[SerializeField] [Tooltip("Max attack range")] private float _attackRange = 10f;

		public Behavior_Shoot()
		{
			_name = Behaviors.SHOOT;
		}

		#region Monobehaviour
		void Awake()//my own singleton pattern, the Singleton.cs doesn't work here as I need multiple behaviors.
		{
			if(s_instance == null)
				s_instance = this;
			else
				Destroy(gameObject);
		}
		#endregion

		#region Behavior
		public override void Execute(Pawn pawn)
		{
			if(s_targetDictionary.ContainsKey(pawn) && s_timerDictionary.ContainsKey(pawn) && null != s_targetDictionary[pawn] && s_targetDictionary[pawn].isActiveAndEnabled)
			{
				if(0 < s_timerDictionary[pawn] || Vector3.Magnitude(s_targetDictionary[pawn].transform.position - pawn.transform.position) < pawn._attackDistance)
				{
					pawn._currentAnimation = PawnAnimations.IDLE;
					s_timerDictionary[pawn]++;//increment timer
					pawn.SetMoveTarget(pawn.transform.position);//stand still
				}
				else
				{
					s_timerDictionary[pawn] = 0;
					if(!pawn._isMounting)
					{
						pawn._currentAnimation = PawnAnimations.RUN;
						pawn.SetMoveTarget(s_targetDictionary[pawn].transform.position);
					}
				}
			}
			else
			{
				s_timerDictionary[pawn] = 0;//or initialise timer
				return;
			}

			if(_attackBuildUpTime <= s_timerDictionary[pawn])//if build up has been finished
			{
				if(s_targetDictionary.ContainsKey(pawn))//check for target
				{
					Pawn target = s_targetDictionary[pawn];
					if(null != target)//check target
					{
						Shoot(pawn, target);
					}
					s_targetDictionary.Remove(pawn);
				}

				s_timerDictionary[pawn] = 0;//reset timer
			}
		}

		public override float FindBestTarget(Pawn pawn)
		{
			float bestScore = 0;

			Pawn lastTarget = pawn;
			bool hadTarget = false;

			if(s_targetDictionary.ContainsKey(pawn))
			{
				lastTarget = s_targetDictionary[pawn];

				if(null != lastTarget && lastTarget.isActiveAndEnabled)
				{
					bestScore = CalculateTargetScore(pawn, lastTarget) + 0.2f;//add flat value to lastTarget (which is the current target)
					hadTarget = true;
				}
			}

			foreach(Pawn target in pawn._activePawns.FindAll(x => x._team != pawn._team))
			{
				float tempScore = CalculateTargetScore(pawn, target);

				if(bestScore < tempScore)
				{
					s_targetDictionary[pawn] = target;//change target if score is better
					bestScore = tempScore;
				}
			}

			if(!hadTarget || (null != lastTarget && s_targetDictionary.ContainsKey(pawn) && lastTarget != s_targetDictionary[pawn]))
				s_timerDictionary[pawn] = 0;//reset timer if target was changed

			return bestScore;
		}

		protected override float PawnAxisInputs(Pawn pawn, string name)
		{
			switch(name)
			{
				case "Health":
					return pawn._health / pawn._maxHealth;
				case "Ammo":
					return (float)pawn._ammo / pawn._maxAmmo;
				case "Cover":
					if(pawn._isMounting)
						return pawn._mountSlot.GetCoverScore(pawn.transform.position);
					else
						return 0f;
				case "Morale":
					return pawn._morale / pawn._maxMorale;
				default:
#if DB_AI
					Debug.LogWarning("PawnAxisInputs defaulted to 1. Probably messed up the string name: " + name);
#endif
					return 1;
			}
		}

		protected float TargetAxisInputs(Pawn pawn, string name, Pawn target)
		{
			switch(name)
			{
				case "Distance":
					return Vector3.Distance(target.transform.position, pawn.transform.position) / 50f;
				case "Health":
					return target._health / target._maxHealth;
				case "Cover":
					if(target._isMounting)
						return target._mountSlot.GetCoverScore(pawn.transform.position);
					else
						return 0f;
				case "DistanceToMyBase":
					HeadQuarter headQuarter = JobCenter.s_headQuarters[pawn._team]?.Find((x) => x.isActiveAndEnabled);
					if(null != headQuarter)
						return Vector3.Distance(s_targetDictionary[pawn].transform.position, headQuarter.transform.position) / 100f;
					else
						return 0.5f;
				case "ShotOnMe":
					return s_targetDictionary.ContainsKey(target) && s_targetDictionary[target] == pawn ? 1f : 0f;
				case "IsMyTarget":
					return s_targetDictionary.ContainsKey(pawn) && s_targetDictionary[pawn] == target ? 1f : 0f;
				default:
					return 1;
			}
		}

		public float CalculateTargetScore(Pawn pawn, Pawn target)
		{
			if(!CheckLos(pawn, target))//early skip when LOS is blocked by something other then cover
				return 0;

			float score = 1f;

			for(int i = 0; i < _targetAxes.Length; i++)
			{
				if(_targetAxes[i]._isEnabled)
				{
					//normal version
					score *= Mathf.Clamp(_targetAxes[i]._curve.Evaluate(TargetAxisInputs(pawn, _targetAxes[i]._name, target)), 0f, 1f);

					//debug version
					//float temp = Mathf.Clamp(_targetAxes[i]._curve.Evaluate(TargetAxisInputs(pawn, _targetAxes[i]._name, target)), 0f, 1f);
					//Debug.Log("Pawn " + pawn._id + " target axis " + _targetAxes[i]._name + " evaluated to " + temp);
					//score *= temp;
				}
			}

			return score;
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
#endregion

		private bool CheckLos(Pawn pawn, Pawn target)
		{
			//check for wall with ray/linecast (+layerMask)

			return true;
		}

		public void Shoot(Pawn pawn, Pawn target)
		{
			if(0 < pawn._ammo)//skip if no ammo
				pawn._ammo--;
			else
				return;

			pawn._arguments |= Arguments.TRIGGERBEHAVIOUR;//set trigger flag for ShootLineController

			if(!CheckLos(pawn, target))
				return;

			if(Random.Range(0f, 1f) < pawn._attackChance)//roll to hit anything
			{
				if(target._isMounting)
				{
					if(Random.Range(0f, 1f) < target._mountSlot.GetCoverScore(pawn.transform.position))//roll to hit cover
					{
						target._mountSlot.GetHit(RollDamage(pawn));
						return;
					}
				}

				target.TakeDamage(RollDamage(pawn));//target hit succesfully
			}
		}

		private int RollDamage(Pawn pawn) => (int)Mathf.Lerp(pawn._minAttackDamage, pawn._maxAttackDamage, Mathf.Clamp(pawn._attackDamageCurve.Evaluate(Random.Range(0f, 1f)), 0f, 1f));
	}
}