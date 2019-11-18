using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace PPBA
{
	public class Behavior_GoAnywhere : Behavior
	{
		public static Behavior_GoAnywhere s_instance;

		[SerializeField] private float _maxDistance = 30f;
		private Vector3 _bestTarget;

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
			pawn.SetMoveTarget(_bestTarget);
		}

		protected override float PawnAxisInputs(Pawn pawn, string name)
		{
			switch(name)
			{
				case "Health":
					return pawn._health / pawn._maxHealth;
				default:
					Debug.LogWarning("PawnAxisInputs defaulted to 1. Probably messed up the string name: " + name);
					return 1;
			}
		}

		protected float TargetAxisInputs(Pawn pawn, string name)
		{
			switch(name)
			{
				case "Distance":
				case "DistanceToTarget":
					return Vector3.Distance(pawn.transform.position, _bestTarget) / _maxDistance;
				default:
					Debug.LogWarning("TargetAxisInputs defaulted to 1. Probably messed up the string name: " + name);
					return 1;
			}
		}

		public override float FindBestTarget(Pawn pawn)
		{
			_bestTarget = GetRandomPoint(pawn);
			_bestTarget = pawn.transform.position + Vector3.forward;
			return 1;
		}

		/// <summary> Gets a random point up to 3 from transform.position </summary>
		protected Vector3 GetRandomPoint(Pawn pawn)
		{
			UnityEngine.AI.NavMeshHit hit;
			Vector2 probe;
			Vector3 probePosition;

			for(int i = 0; i < 64; i++)
			{
				probe = Random.insideUnitCircle * 3f;
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

			return transform.position;
		}

		public override int GetTargetID(Pawn pawn)
		{
			throw new System.NotImplementedException();
		}
	}
}