using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public abstract class MountSlot : MonoBehaviour
	{
		//public
		[SerializeField] public int _id = 0;
		[SerializeField] public int _team = 0;

		[SerializeField] [Tooltip("How fit is the MountSlot right now?")] public float _score = 0;
		[SerializeField] [Tooltip("How much cover does the MountSlot offer a pawn?")] public float _coverScore = 0f;
		public bool _isMounted => _mountingPawn != null && _mountingPawn.isActiveAndEnabled;
		[SerializeField] public Pawn _mountingPawn = null;

		#region Pawn Interaction
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

		public virtual void GetHit(int amount)
		{
			Debug.Log("Hitting MountSlot " + gameObject.name + " did not apply damage anywhere.");
		}
		#endregion

		#region Tick
		public abstract void Execute();
		public abstract void CalculateScore(int tick = 0);//add to action: s_LateCalc (attention: can't use cover scores as they are calculated at the same time. maybe move those to EarlyCalc)

		public virtual float GetCoverScore(Vector3 shooterPosition)
		{
			return _coverScore;
		}

		public virtual void WriteToGameState(int tick)
		{
			//new GSC.arg { _arguments = Arguments.ENABLED, _id = 0 };
			//IDictionary team health trans mounting pawn

			//TickHandler.s_interfaceGameState.Add(new GSC.transform { _id = _id, _position = transform.position, _angle = transform.eulerAngles.y });
		}
		#endregion

		private void OnEnable()
		{
#if UNITY_SERVER
			if(null != JobCenter.s_mountSlots && _team < JobCenter.s_mountSlots.Length && !JobCenter.s_mountSlots[_team].Contains(this))
				JobCenter.s_mountSlots[_team].Add(this);

			TickHandler.s_LateCalc += CalculateScore;
			//TickHandler.s_GatherValues += WriteToGameState;
#endif
		}

		private void OnDisable()
		{
#if UNITY_SERVER
			if(null != JobCenter.s_mountSlots && JobCenter.s_mountSlots[_team].Contains(this))
				JobCenter.s_mountSlots[_team].Remove(this);

			if(_isMounted)
				Behavior_Mount.s_instance.RemoveFromTargetDict(_mountingPawn);

			TickHandler.s_LateCalc -= CalculateScore;
			//TickHandler.s_GatherValues -= WriteToGameState;
#endif
		}
	}
}
