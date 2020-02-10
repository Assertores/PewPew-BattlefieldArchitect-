using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace PPBA
{
	public class Behavior_GoAnywhere : Behavior
	{
		//public
		public static Behavior_GoAnywhere s_instance;
		public static Dictionary<Pawn, Vector3> s_targetDictionary = new Dictionary<Pawn, Vector3>();

		//private
		[SerializeField] private float _maxDistance = 30f;

		public Behavior_GoAnywhere()
		{
			_name = Behaviors.GOANYWHERE;
		}

		private void Awake()
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
			if(s_targetDictionary.ContainsKey(pawn))
			{
				pawn._currentAnimation = PawnAnimations.RUN;
				pawn.SetMoveTarget(s_targetDictionary[pawn]);
			}
			else
			{
				pawn._currentAnimation = PawnAnimations.IDLE;
				pawn.SetMoveTarget(pawn.transform.position);
			}

			//pawn.SetMoveTarget(new Vector3(-135f, 0f, 11f));
		}

		protected override float PawnAxisInputs(Pawn pawn, string name)
		{
			switch(name)
			{
				case "Health":
					return pawn._health / pawn._maxHealth;
				default:
#if DB_AI
					Debug.LogWarning("PawnAxisInputs defaulted to 1. Probably messed up the string name: " + name);
#endif
					return 1;
			}
		}

		protected float TargetAxisInputs(Pawn pawn, Vector3 target, string name)
		{
			switch(name)
			{
				case "Distance":
				case "DistanceToTarget":
					return Vector3.Distance(pawn.transform.position, target) / _maxDistance;
				default:
#if DB_AI
					Debug.LogWarning("TargetAxisInputs defaulted to 1. Probably messed up the string name: " + name);
#endif
					return 1;
			}
		}

		public override float FindBestTarget(Pawn pawn)
		{
			/*
			if(s_targetDictionary.ContainsKey(pawn))
			{
				if(pawn.transform.position != s_targetDictionary[pawn])
					return 1f;
			}
			*/
			//s_targetDictionary[pawn] = GetRandomPoint(pawn);
			s_targetDictionary[pawn] = pawn.transform.position + 3f * new Vector3(Mathf.Sin(TickHandler.s_currentTick), 0f, Mathf.Cos(TickHandler.s_currentTick));
			//s_targetDictionary[pawn] = pawn.transform.position + Vector3.forward;
			return 1f;
		}

		/// <summary> Gets a random point up to 3 from transform.position </summary>
		protected Vector3 GetRandomPoint(Pawn pawn)
		{
			UnityEngine.AI.NavMeshHit hit;
			Vector2 probe;
			Vector3 probePosition;

			for(int i = 0; i < 32; i++)
			{
				probe = Random.insideUnitCircle * 10f;
				probePosition = new Vector3(transform.position.x + probe.x, transform.position.y, transform.position.z + probe.y);

				if(UnityEngine.AI.NavMesh.SamplePosition(probePosition, out hit, 0.1f, NavMesh.AllAreas))
					return hit.position;
				else
				{   //checks the same point in the opposite direction
					probePosition = new Vector3(-probePosition.x, probePosition.y, -probePosition.z);
					if(UnityEngine.AI.NavMesh.SamplePosition(probePosition, out hit, 0.1f, NavMesh.AllAreas))
						return hit.position;
				}
			}

			return pawn.transform.position;
		}

		public override int GetTargetID(Pawn pawn)
		{
			//empty because no target dictionary
			return -1;
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