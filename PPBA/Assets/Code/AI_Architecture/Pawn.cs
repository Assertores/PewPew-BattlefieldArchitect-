using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace PPBA
{
	public enum Behaviors : byte { IDLE, SHOOT, THROWGRENADE, GOTOFLAG, GOTOBORDER, CONQUERBUILDING, STAYINCOVER, GOTOCOVER, GOTOHEAL, FLEE, GETRESOURCES, BRINGRESOURCES, BUILD, DECONSTRUCT, GETAMMO, MOUNT, FOLLOW, DIE, WINCHEER, GOANYWHERE };

	[RequireComponent(typeof(NavMeshAgent))]
	[RequireComponent(typeof(SphereCollider))]
	public abstract class Pawn : MonoBehaviour
	{
		//Behaviors
		//protected enum Behavior { GoAnywhere, Shoot, Heal };
		[SerializeField] protected Behaviors[] e_behaviors;
		protected Behavior[] behaviors;
		[SerializeField] [Tooltip("Displays last calculated behavior-scores.\nNo reason to change these.")] protected float[] behavior_scores;

		//public
		[SerializeField] public int team;

		//protected
		[SerializeField] protected bool isAttacking = false;

		[SerializeField] public float health = 100;
		[SerializeField] public float maxHealth = 100;

		[SerializeField] public float ammo = 100;
		[SerializeField] public float maxAmmo = 100;

		[SerializeField] public float morale = 100;
		[SerializeField] public float maxMorale = 100;

		[SerializeField] public float attackDistance = 5f;
		[SerializeField] public float attackDamage = 2f;

		//Components
		//[HideInInspector]
		public NavMeshAgent navMeshAgent;
		private SphereCollider sphereCollider;

		private object target;
		private int resource;   //resources carried

		//target lists
		// pawns, covers, depots, bringjobs, buildjobs, deconstructjobs
		public List<Pawn> closePawns;
		public List<Cover> closeCover;

		// Start is called before the first frame update
		public void Start()
		{
			InitiateBehaviors();
			navMeshAgent = GetComponent<NavMeshAgent>();
			sphereCollider = GetComponent<SphereCollider>();
			StartCoroutine(DoTick());
		}

		public void Update()
		{

		}

		protected void Evaluate(int tick = 0)   //uses behavior-scores to evaluate behaviors
		{
			for(int i = 0; i < behaviors.Length; i++)
			{
				behavior_scores[i] = behaviors[i].Calculate(this);
			}
		}

		protected void Execute(int tick = 0)   //calls the execution on the most appropriate behavior
		{
			int best_behavior = 0;
			float best_score = 0;

			for(int i = 0; i < behavior_scores.Length; i++)//determines best behavior
			{
				if(best_score < behavior_scores[i])
				{
					best_behavior = i;
					best_score = behavior_scores[i];
				}
			}

			behaviors[best_behavior].Execute(this);
		}

		#region Initialisation
		protected void InitiateBehaviors()  //reads the behaviors from the enum-array
		{
			behaviors = new Behavior[e_behaviors.Length];
			behavior_scores = new float[e_behaviors.Length];

			for(int i = 0; i < e_behaviors.Length; i++)
			{
				behaviors[i] = GetBehavior(e_behaviors[i]);
			}
		}

		protected Behavior GetBehavior(Behaviors e_behaviors)   //switch used for initialising behavior-array
		{
			switch(e_behaviors)
			{
				case Behaviors.IDLE:
					return Behavior_Idle.instance;
				case Behaviors.SHOOT:
					return Behavior_Shoot.instance;
				case Behaviors.THROWGRENADE:
					break;
				case Behaviors.GOTOFLAG:
					break;
				case Behaviors.GOTOBORDER:
					break;
				case Behaviors.CONQUERBUILDING:
					break;
				case Behaviors.STAYINCOVER:
					break;
				case Behaviors.GOTOCOVER:
					break;
				case Behaviors.GOTOHEAL:
					return Behavior_GoAnywhere.instance;
				case Behaviors.FLEE:
					break;
				case Behaviors.GETRESOURCES:
					break;
				case Behaviors.BRINGRESOURCES:
					break;
				case Behaviors.BUILD:
					break;
				case Behaviors.DECONSTRUCT:
					break;
				case Behaviors.GETAMMO:
					break;
				case Behaviors.MOUNT:
					break;
				case Behaviors.FOLLOW:
					break;
				case Behaviors.DIE:
					break;
				case Behaviors.WINCHEER:
					break;
				case Behaviors.GOANYWHERE:
					return Behavior_GoAnywhere.instance;
				default:
					Debug.LogWarning("GetBehavior switch defaulted. Couldn't get the desired behavior.");
					return Behavior_GoAnywhere.instance;
			}

			return Behavior_GoAnywhere.instance;
		}

		private void OnEnable()
		{			
			TickHandler.s_AIEvaluate += Evaluate;
			TickHandler.s_DoTick += Execute;
		}

		private void OnDisable()
		{
			TickHandler.s_AIEvaluate -= Evaluate;
			TickHandler.s_DoTick -= Execute;
		}
		#endregion

		#region FakeTick
		protected IEnumerator DoTick()
		{
			while(enabled)
			{
				yield return new WaitForSeconds(0.2f);
				Evaluate();
				Execute();
			}
		}
		#endregion

		#region Physics
		private void OnTriggerEnter(Collider other)
		{
			//Add relevant objects to closeLists
			if(other.tag == "Pawn")
			{
				Pawn temp = other.gameObject.GetComponent<Pawn>();
				if(temp)
					closePawns.Add(temp);
			}

			if(other.tag == "Cover")
			{
				Cover temp = other.gameObject.GetComponent<Cover>();
				if(temp)
					closeCover.Add(temp);
			}
		}

		private void OnTriggerExit(Collider other)
		{
			//Remove objects from closeLists
			if(other.tag == "Pawn")
			{
				Pawn temp = other.gameObject.GetComponent<Pawn>();
				if(temp && closePawns.Contains(temp))
					closePawns.Remove(temp);
			}

			if(other.tag == "Cover")
			{
				Cover temp = other.gameObject.GetComponent<Cover>();
				if(temp && closeCover.Contains(temp))
					closeCover.Remove(temp);
			}
		}
		#endregion

		#region Gizmos
		private void OnDrawGizmos()
		{
			Gizmos.color = Color.blue;
			Gizmos.DrawLine(transform.position, navMeshAgent.destination);
		}
		#endregion
	}
}