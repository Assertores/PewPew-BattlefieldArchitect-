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
		//public
		[SerializeField] public int _id = 0;
		[SerializeField] public int _team = 0;

		[SerializeField] public float _health = 100;
		[SerializeField] public float _maxHealth = 100;

		[SerializeField] public int _ammo = 100;
		[SerializeField] public int _maxAmmo = 100;

		[SerializeField] public float _morale { get => _moraleBackingField; set => Mathf.Clamp(value, 0, _maxMorale); }
		[SerializeField] public float _maxMorale = 100;

		[SerializeField] public float _attackDistance = 5f;
		[SerializeField] public float _attackDamage = 2f;

		[SerializeField] public int _resources;//resources carried
		[SerializeField] public int _maxResource = 10;

		//protected
		[SerializeField] protected bool _isAttacking = false;

		//Behaviors
		//protected enum Behavior { GoAnywhere, Shoot, Heal };
		[SerializeField] protected Behaviors[] e_behaviors;
		protected Behavior[] _behaviors;
		protected Behavior _lastBehavior;
		[SerializeField] [Tooltip("Displays last calculated behavior-scores.\nNo reason to change these.")] protected float[] _behaviorScores;

		//Components
		public NavMeshAgent _navMeshAgent;
		private SphereCollider _sphereCollider;

		//target lists
		public List<Pawn> _closePawns;
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
		public List<Cover> _closeCover;

		//private
		private float _moraleBackingField = 100;

		public void Start()
		{
			InitiateBehaviors();
			_navMeshAgent = GetComponent<NavMeshAgent>();
			_sphereCollider = GetComponent<SphereCollider>();

			StartCoroutine(DoTick());//fake tick
		}

		public void Update()
		{

		}

		protected void Evaluate(int tick = 0)   //uses behavior-scores to evaluate behaviors
		{
			for(int i = 0; i < _behaviors.Length; i++)
			{
				_behaviorScores[i] = _behaviors[i].Calculate(this);
			}
		}

		protected void Execute(int tick = 0)   //calls the execution on the most appropriate behavior
		{
			int bestBehavior = 0;
			float bestScore = 0;

			if(_health <= 0)
			{
				Behavior_Die.s_instance.Execute(this);
				return;
			}

			for(int i = 0; i < _behaviorScores.Length; i++)//determines best behavior
			{
				if(bestScore < _behaviorScores[i])
				{
					bestBehavior = i;
					bestScore = _behaviorScores[i];
				}
			}

			if(_lastBehavior != _behaviors[bestBehavior])//on behavior change
			{

			}

			_lastBehavior = _behaviors[bestBehavior];
			_lastBehavior.Execute(this);
		}

		#region Initialisation
		//public void InitialisePawn(PARAMETER)

		protected void InitiateBehaviors()  //reads the behaviors from the enum-array
		{
			_behaviors = new Behavior[e_behaviors.Length];
			_behaviorScores = new float[e_behaviors.Length];

			for(int i = 0; i < e_behaviors.Length; i++)
			{
				_behaviors[i] = GetBehavior(e_behaviors[i]);
			}
		}

		protected Behavior GetBehavior(Behaviors e_behaviors)   //switch used for initialising behavior-array
		{
			switch(e_behaviors)
			{
				case Behaviors.IDLE:
					return Behavior_Idle.instance;
				case Behaviors.SHOOT:
					return Behavior_Shoot.s_instance;
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
			TickHandler.s_GatherValues += WriteToGameState;
		}

		private void OnDisable()
		{
			TickHandler.s_AIEvaluate -= Evaluate;
			TickHandler.s_DoTick -= Execute;
			TickHandler.s_GatherValues -= WriteToGameState;
		}
		#endregion

		#region Member Admin
		public void TakeDamage(int amount)
		{
			_health -= amount;
			//set "i got hurt" flag to send to the client

			if(_health <= 0)
				Behavior_Die.s_instance.Execute(this);
			else
			{
				foreach(Pawn p in _closePawns)
				{
					if(_team == p._team)
						p._morale += Moralizer.s_instance.PassJudgement(MoraleEvents.CLOSEPAWNDAMAGED);
					else
						p._morale += Moralizer.s_instance.PassJudgement(MoraleEvents.ENEMYDAMAGED);
				}
			}

			_morale -= amount;
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

		public void WriteToGameState(int tick)
		{
			//new GSC.arg { _arguments = Arguments.ENABLED, _id = 0 };

			//TickHandler.s_interfaceGameState._types = new List<GSC.type>();
			//TickHandler.s_interfaceGameState._args = new List<GSC.arg>();
			TickHandler.s_interfaceGameState._transforms.Add(new GSC.transform(_id, transform.position, transform.eulerAngles.y));
			TickHandler.s_interfaceGameState._ammos.Add(new GSC.ammo(_id, _ammo));
			TickHandler.s_interfaceGameState._resources.Add(new GSC.resource(_id, _resources));
			TickHandler.s_interfaceGameState._healths.Add(new GSC.health(_id, _health, _morale));
			TickHandler.s_interfaceGameState._behaviors.Add(new GSC.behavior(_id, _lastBehavior));//this doesn't give a taget yet
			TickHandler.s_interfaceGameState._paths.Add(new GSC.path(_id, _navMeshAgent.path.corners));
		}

		#region Physics
		private void OnTriggerEnter(Collider other)
		{
			//Add relevant objects to closeLists
			if(other.tag == "Pawn")
			{
				Pawn temp = other.gameObject.GetComponent<Pawn>();
				if(temp)
					_closePawns.Add(temp);
			}

			if(other.tag == "Cover")
			{
				Cover temp = other.gameObject.GetComponent<Cover>();
				if(temp)
					_closeCover.Add(temp);
			}
		}

		private void OnTriggerExit(Collider other)
		{
			//Remove objects from closeLists
			if(other.tag == "Pawn")
			{
				Pawn temp = other.gameObject.GetComponent<Pawn>();
				if(temp && _closePawns.Contains(temp))
					_closePawns.Remove(temp);
			}

			if(other.tag == "Cover")
			{
				Cover temp = other.gameObject.GetComponent<Cover>();
				if(temp && _closeCover.Contains(temp))
					_closeCover.Remove(temp);
			}
		}
		#endregion

		#region Gizmos
		private void OnDrawGizmos()
		{
			Gizmos.color = Color.blue;
			Gizmos.DrawLine(transform.position, _navMeshAgent.destination);
		}
		#endregion
	}
}