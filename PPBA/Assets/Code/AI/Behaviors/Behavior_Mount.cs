using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class Behavior_Mount : Behavior
	{
		public static Behavior_Mount s_instance;
		public static Dictionary<Pawn, MountSlot> s_targetDictionary = new Dictionary<Pawn, MountSlot>();

		[SerializeField] private float _maxDistance = 70f;
		[SerializeField] [Tooltip("How close does a pawn have to be to allow mounting?")] private float _mountDistance = .5f;

		public Behavior_Mount()
		{
			_name = Behaviors.MOUNT;
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
			pawn._currentAnimation = PawnAnimations.IDLE;

			if(!s_targetDictionary.ContainsKey(pawn))//early skip if no target
			{
				pawn.SetMoveTarget(pawn.transform.position);
				return;
			}

			if(pawn._isMounting)
			{
				pawn._mountSlot.Execute();
				pawn.SetMoveTarget(pawn._mountSlot.transform.position);
			}
			else
			{
				if(s_targetDictionary[pawn]._isMounted)
				{
					pawn.SetMoveTarget(pawn.transform.position);
					return;
				}

				float distance = Vector3.Magnitude(s_targetDictionary[pawn].transform.position - pawn.transform.position);

				if(distance < _mountDistance)
				{
					s_targetDictionary[pawn].GetIn(pawn);
					pawn.SetMoveTarget(pawn._mountSlot.transform.position);
					return;
				}

				pawn._currentAnimation = PawnAnimations.RUN;
				pawn.SetMoveTarget(s_targetDictionary[pawn].transform.position);
			}
		}

		public override float FindBestTarget(Pawn pawn)
		{
			float bestScore = 0f;

			foreach(MountSlot slot in JobCenter.s_mountSlots[pawn._team])
			{
				if(!slot.isActiveAndEnabled || slot._isMounted)
					continue;

				float tempScore = CalculateTargetScore(pawn, slot);

				if(bestScore < tempScore)
				{
					s_targetDictionary[pawn] = slot;
					bestScore = tempScore;
				}
			}

			return bestScore;
		}

		protected float CalculateTargetScore(Pawn pawn, MountSlot mountSlot)
		{
			float _score = 1;

			for(int i = 0; i < _targetAxes.Length; i++)
			{
				if(_targetAxes[i]._isEnabled)
					_score *= Mathf.Clamp(_targetAxes[i]._curve.Evaluate(TargetAxisInputs(pawn, _targetAxes[i]._name, mountSlot)), 0f, 1f);
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

		protected override float PawnAxisInputs(Pawn pawn, string name)
		{
			switch(name)
			{
				case "Health":
					return pawn._health / pawn._maxHealth;
				default:
					return 1f;
			}
		}

		protected float TargetAxisInputs(Pawn pawn, string name, MountSlot mountSlot)
		{
			switch(name)
			{
				case "Distance":
					return Vector3.Distance(pawn.transform.position, mountSlot.transform.position) / _maxDistance;
				case "Score":
					return mountSlot._coverScore;
				default:
					return 1f;
			}
		}

		public override void RemoveFromTargetDict(Pawn pawn)
		{
			if(s_targetDictionary.ContainsKey(pawn))
			{
				s_targetDictionary.Remove(pawn);
			}

			pawn._mountSlot?.GetOut(pawn);
		}
	}
}