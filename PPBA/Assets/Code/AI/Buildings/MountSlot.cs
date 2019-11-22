using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	[RequireComponent(typeof(Collider))]
	public abstract class MountSlot : MonoBehaviour
	{
		//public
		[SerializeField] public int _id = 0;
		[SerializeField] public int _team = 0;
		[SerializeField] public float _health { get => _healthBackingField; set => _healthBackingField = Mathf.Clamp(value, 0, _maxHealth); }
		[SerializeField] public float _maxHealth = 100;

		[SerializeField] [Tooltip("How fit is the MountSlot right now?")] public float _score = 0;
		[SerializeField] [Tooltip("How much cover does the MountSlot offer a pawn?")] public float _coverScore = 0f;
		public bool _isMounted => _mountingPawn != null;
		[SerializeField] public Pawn _mountingPawn = null;
		public List<Pawn> _activePawns//accessor to _closePawns
		{
			get
			{
				foreach(var it in _closePawns)
				{
					if(!it.gameObject.activeSelf)
						_closePawns.Remove(it);//inactive pawns are removed here, the other option would be to go through the lists of all pawns whenever a pawn is disabled to remove it from the lists
				}
				return _closePawns;
			}
		}
		public List<Pawn> _closePawns = new List<Pawn>();

		//protected
		protected float _healthBackingField = 100;
		
		public bool GetIn(Pawn pawn)
		{
			if(_isMounted)
				return false;
			else
			{
				_mountingPawn = pawn;
				pawn._mountSlot = this;
				return true;
			}
		}

		public bool GetOut(Pawn pawn)
		{
			if(_mountingPawn == pawn)
			{
				_mountingPawn = null;
				pawn._mountSlot = null;
				return true;
			}
			else
				return false;
		}

		public abstract void Execute();
		public abstract void CalculateScore(int tick = 0);//add to action: s_LateCalc (attention: can't use cover scores as they are calculated at the same time. maybe move those to EarlyCalc)

		public void TakeDamage(int amount)
		{
			_health -= amount;
			//set "i got hurt" flag to send to the client

			if(_health <= 0)
			{
				//die
			}
		}

		public void OnTriggerEnter(Collider other)
		{
			//Add relevant objects to closeLists
			if(other.tag == "Pawn")
			{
				Pawn temp = other.gameObject.GetComponent<Pawn>();
				if(temp)
					_closePawns.Add(temp);
			}
		}

		public void OnTriggerExit(Collider other)
		{
			//Remove objects from closeLists
			if(other.tag == "Pawn")
			{
				Pawn temp = other.gameObject.GetComponent<Pawn>();
				if(temp && _closePawns.Contains(temp))
					_closePawns.Remove(temp);
			}
		}

		private void OnEnable()
		{
			if(!JobCenter.s_mountSlots[_team].Contains(this))
				JobCenter.s_mountSlots[_team].Add(this);

			TickHandler.s_LateCalc += CalculateScore;
			//TickHandler.s_GatherValues += WriteToGameState;
		}

		private void OnDisable()
		{
			if(JobCenter.s_mountSlots[_team].Contains(this))
				JobCenter.s_mountSlots[_team].Remove(this);

			if(_isMounted)
				Behavior_Mount.s_instance.RemoveFromTargetDict(_mountingPawn);

			TickHandler.s_LateCalc -= CalculateScore;
			//TickHandler.s_GatherValues -= WriteToGameState;
		}
	}
}