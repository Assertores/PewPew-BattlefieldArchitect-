using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	[RequireComponent(typeof(Collider))]
	public abstract class MountSlot : MonoBehaviour
	{
		//Fields
		[SerializeField] public Pawn _mountingPawn = null;
		[SerializeField] [Tooltip("How fit is the MountSlot right now?")] public float _score = 0;
		[SerializeField] [Tooltip("How much cover does the MountSlot offer a pawn?")] public float _coverScore = 0f;
		public List<Pawn> _closePawns = new List<Pawn>();
		public List<Pawn> _activePawns//this is an accessor to the _closePawns List, ensuring I don't have to write this every time I want to use the list.
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

		//Parameters
		public bool _isMounted => _mountingPawn == null;

		public bool GetIn(Pawn pawn)
		{
			if(_isMounted)
				return false;
			else
			{
				_mountingPawn = pawn;
				return true;
			}
		}

		public bool GetOut(Pawn pawn)
		{
			if(_mountingPawn == pawn)
			{
				_mountingPawn = null;
				return true;
			}
			else
				return false;
		}

		public abstract void Execute();
		public abstract void CalculateScore(int tick = 0);//add to action: s_LateCalc (attention: can't use cover scores as they are calculated at the same time. maybe move those to EarlyCalc)

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
	}
}