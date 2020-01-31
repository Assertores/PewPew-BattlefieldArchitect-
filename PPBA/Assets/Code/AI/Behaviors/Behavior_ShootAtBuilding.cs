using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class Behavior_ShootAtBuilding : Behavior
	{
		//public
		public static Behavior_ShootAtBuilding s_instance;
		public static Dictionary<Pawn, IDestroyableBuilding> s_targetDictionary = new Dictionary<Pawn, IDestroyableBuilding>();
		public static Dictionary<Pawn, int> s_timerDictionary = new Dictionary<Pawn, int>();

		//private
		[SerializeField] [Tooltip("How long from starting the attack to shooting in ticks?")] private int _attackBuildUpTime = 8;
		[SerializeField] [Tooltip("Max attack range")] private float _attackRange = 10f;

		public Behavior_ShootAtBuilding()
		{
			_name = Behaviors.SHOOTATBUILDING;
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
			if(s_targetDictionary.ContainsKey(pawn) && s_timerDictionary.ContainsKey(pawn) && null != s_targetDictionary[pawn] && s_targetDictionary[pawn].GetTransform().gameObject.activeInHierarchy)
			{
				Vector3 targetPosition = s_targetDictionary[pawn].GetTransform().position;

				if(0 < s_timerDictionary[pawn] || Vector3.Magnitude(targetPosition - pawn.transform.position) < pawn._attackDistance)
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
						pawn.SetMoveTarget(targetPosition);
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
					IDestroyableBuilding target = s_targetDictionary[pawn];
					if(null != target)//check target
					{
						Shoot(pawn, target);
					}
					s_targetDictionary.Remove(pawn);//evtl zu früh removed für visualisierung?
				}

				s_timerDictionary[pawn] = 0;//reset timer
			}
		}

		public override float FindBestTarget(Pawn pawn)
		{
			float bestScore = 0;

			IDestroyableBuilding lastTarget = null;
			bool hadTarget = false;

			if(s_targetDictionary.ContainsKey(pawn))
			{
				lastTarget = s_targetDictionary[pawn];

				if(null != lastTarget && lastTarget.GetTransform().gameObject.activeInHierarchy)
				{
					bestScore = CalculateTargetScore(pawn, lastTarget) + 0.2f;//add flat value to lastTarget (which is the current target)
					hadTarget = true;
				}
			}

			foreach(IDestroyableBuilding target in pawn._activeBuildings.FindAll(x => x.GetTeam() != pawn._team))
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

		protected float TargetAxisInputs(Pawn pawn, string name, IDestroyableBuilding target)
		{
			switch(name)
			{
				case "Distance":
					return Vector3.Distance(target.GetTransform().position, pawn.transform.position) / 50f;
				case "Health":
					return target.GetHealth() / target.GetMaxHealth();
				case "DistanceToMyBase":
					HeadQuarter headQuarter = JobCenter.s_headQuarters[pawn._team]?.Find((x) => x.isActiveAndEnabled);
					if(null != headQuarter)
						return Vector3.Distance(target.GetTransform().position, headQuarter.transform.position) / 100f;
					else
						return 0.5f;
				case "IsMyTarget":
					return s_targetDictionary.ContainsKey(pawn) && s_targetDictionary[pawn] == target ? 1f : 0f;
				default:
					return 1;
			}
		}

		public float CalculateTargetScore(Pawn pawn, IDestroyableBuilding target)
		{
			//if(!CheckLos(pawn, target))//early skip when LOS is blocked by something other then cover
				//return 0;

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

		public void Shoot(Pawn pawn, IDestroyableBuilding target)
		{
			if(0 < pawn._ammo)//skip if no ammo
				pawn._ammo--;
			else
				return;

			pawn._arguments |= Arguments.TRIGGERBEHAVIOUR;//set trigger flag for ShootLineController

			//if(!CheckLos(pawn, target))
			//return;

			if(Random.Range(0f, 1f) < pawn._attackChance)
				target.TakeDamage(RollDamage(pawn));//target hit succesfully
		}

		private int RollDamage(Pawn pawn) => (int)Mathf.Lerp(pawn._minAttackDamage, pawn._maxAttackDamage, Mathf.Clamp(pawn._attackDamageCurve.Evaluate(Random.Range(0f, 1f)), 0f, 1f));
	}
}