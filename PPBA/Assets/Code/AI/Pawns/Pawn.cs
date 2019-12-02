﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using TMPro;

namespace PPBA
{
	public enum Behaviors : byte { IDLE, SHOOT, THROWGRENADE, GOTOFLAG, GOTOBORDER, CONQUERBUILDING, STAYINCOVER, GOTOCOVER, GOTOHEAL, FLEE, GETRESOURCES, BRINGRESOURCES, BUILD, DECONSTRUCT, GETAMMO, MOUNT, FOLLOW, DIE, WINCHEER, GOANYWHERE };

	[RequireComponent(typeof(SphereCollider))]
	[RequireComponent(typeof(LineRenderer))]
	public class Pawn : MonoBehaviour, IPanelInfo, INetElement
	{
		#region Variables
		//public
		public int _id { get; set; }
		[SerializeField] public int _team = 0;

		//[SerializeField] public float _health = 100;
		[SerializeField] public float _health { get => _healthBackingField; set => _healthBackingField = Mathf.Clamp(value, 0, _maxHealth); }
		[SerializeField] public float _maxHealth = 100;

		[SerializeField] public int _ammo { get => _ammoBackingField; set => _ammoBackingField = Mathf.Clamp(value, 0, _maxAmmo); }
		[SerializeField] public int _maxAmmo = 100;

		[SerializeField] public float _morale { get => _moraleBackingField; set => _moraleBackingField = Mathf.Clamp(value, 0, _maxMorale); }
		[SerializeField] public float _maxMorale = 100;

		[SerializeField] public float _attackDistance = 5f;
		[SerializeField] [Tooltip("0: Never hits.\n 1: Always hits.")] [Range(0f, 1f)] public float _attackChance = 0.5f;
		[SerializeField] public float _minAttackDamage = 25f;
		[SerializeField] public float _maxAttackDamage = 75f;
		[SerializeField] [Tooltip("How far to lerp from _minAttackDamage to _maxAttackDamage depending on random number between 0f and 1?")] public AnimationCurve _attackDamageCurve;

		[SerializeField] public int _resources;//resources carried
		[SerializeField] public int _maxResource = 10;

		[HideInInspector] public bool _isNavPathDirty = true;//refresh every tick

		//protected
		[SerializeField] protected bool _isAttacking = false;
		[SerializeField] [Range(1f, 10f)] private float _moveSpeed = 1f;
		protected float _moraleBackingField = 100;
		protected float _healthBackingField = 100;
		protected int _ammoBackingField = 100;
		[SerializeField] [Tooltip("Health regeneration per tick.")] protected float _healthRegen = 1f;
		[SerializeField] [Tooltip("Morale regeneration per tick.")] protected float _moraleRegen = 1f;

		#endregion

		#region References
		//Behaviors
		[SerializeField] protected Behaviors[] e_behaviors;
		protected Behavior[] _behaviors;
		protected Behavior _lastBehavior = Behavior_Idle.s_instance;
		[SerializeField] [Tooltip("Displays last calculated behavior-scores.\nNo reason to change these.")] protected float[] _behaviorScores;

		//Components
		public NavMeshPath _navMeshPath;
		private SphereCollider _sphereCollider;
		private LineRenderer _lineRenderer;
		[SerializeField] private GameObject _objectToFollow;//to test navAgent

		//targets and target lists
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
		public List<CoverSlot> _closeCoverSlots;
		public List<CoverSlot> _activeCoverSlots
		{
			get
			{
				foreach(var it in _closeCoverSlots)
				{
					if(!it.gameObject.activeSelf)
						_closeCoverSlots.Remove(it);//inactive covers are removed here
				}
				return _closeCoverSlots;
			}
		}
		public Vector3 _moveTarget;//let the behaviors set this
		public MountSlot _mountSlot = null;
		public bool _isMounting => _mountSlot != null;
		#endregion

		#region MonoBehaviour
		public void Start()
		{
			//Get references
			_navMeshPath = new NavMeshPath();
			_sphereCollider = GetComponent<SphereCollider>();
			_lineRenderer = GetComponent<LineRenderer>();

			//Initialisation
			InitiateBehaviors();

			StartCoroutine(DoTick());//fake tick
		}

		public void Update()
		{
			//if(_objectToFollow != null && _navMeshAgent != null)
			//{
			//	//NavMesh.CalculatePath(transform.position, _objectToFollow.transform.position, _navMeshAgent.areaMask, _navMeshAgent.path);
			//	_navMeshAgent.SetDestination(_objectToFollow.transform.position);
			//}

			ShowNavPath();
			SetMoveTarget(_objectToFollow.transform.position);
		}

		private void FixedUpdate()
		{
			NavTick();
		}
		#endregion

		#region Tick
		protected void Evaluate(int tick = 0)   //uses behavior-scores to evaluate behaviors
		{
			CheckOverlapSphere();

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

			if(0 < _behaviors.Length)
			{
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
					_lastBehavior.RemoveFromTargetDict(this);//remove from lastBehaviors targetList

					//change animation
				}

				_lastBehavior = _behaviors[bestBehavior];
				_lastBehavior.Execute(this);
			}

			NavTick();
			Regenerate();
		}

		public void WriteToGameState(int tick)
		{
			//new GSC.arg { _arguments = Arguments.ENABLED, _id = 0 };

			TickHandler.s_interfaceGameState._transforms.Add(new GSC.transform { _id = _id, _position = transform.position, _angle = transform.eulerAngles.y });
			TickHandler.s_interfaceGameState._ammos.Add(new GSC.ammo { _id = _id, _bullets = _ammo });
			TickHandler.s_interfaceGameState._resources.Add(new GSC.resource { _id = _id, _resources = _resources });
			TickHandler.s_interfaceGameState._healths.Add(new GSC.health { _id = _id, _health = _health, _morale = _morale });
			if(_lastBehavior != null)
				TickHandler.s_interfaceGameState._behaviors.Add(new GSC.behavior { _id = _id, _behavior = GetBehaviorsEnum(_lastBehavior), _target = _lastBehavior.GetTargetID(this) });//this doesn't give a target yet
			if(_navMeshPath != null)
				TickHandler.s_interfaceGameState._paths.Add(new GSC.path { _id = _id, _path = _navMeshPath.corners });
		}

		public static void ExtractFromGameState(int tick)
		{
			GameState myGS = new GameState();
		}

		/*
		void HandleGameStateEnableEvents(int tick)
		{
			Arguments args = TickHandler.s_interfaceGameState._args.Find(x => x._id == _id)._arguments;
			if(args.HasFlag(Arguments.ENABLED))
			{
				if(!this.gameObject.activeSelf)
				{
					_holder.gameObject.SetActive(true);

					GSC.transform newTransform = TickHandler.s_interfaceGameState._transforms.Find(x => x._id == _id);

					_holder.transform.position = newTransform._position;
					_holder.transform.rotation = Quaternion.Euler(0, newTransform._angle, 0);
				}
			}
			else
			{
				if(this.gameObject.activeSelf)
				{
					_holder.gameObject.SetActive(false);
				}
			}
		}
		*/
		#endregion

		#region Initialisation
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
					return Behavior_Idle.s_instance;
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
					return Behavior_GoAnywhere.s_instance;
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
					return Behavior_GoAnywhere.s_instance;
				default:
					Debug.LogWarning("GetBehavior switch defaulted. Couldn't get the desired behavior.");
					return Behavior_GoAnywhere.s_instance;
			}

			return Behavior_GoAnywhere.s_instance;
		}

		/// <summary>
		/// Takes a typeof(Behavior) and returns the corresponding enum
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public Behaviors GetBehaviorsEnum(Behavior behavior)
		{
			/*
			switch(typeof(behavior))
			{
				case typeof(Behavior_Idle):
					return Behaviors.IDLE;
				case typeof(Behavior_Shoot):
				return Behaviors.SHOOT;
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
					break;
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
					break;
				default:
					break;
			}
			*/
			return Behaviors.IDLE;
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

		public void Heal(int amount)
		{
			_health = Mathf.Min(_health + amount, _maxHealth);
		}

		public void Regenerate()
		{
			_health += _healthRegen;
			_morale += _moraleRegen;
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

		#region Navigation
		public void NavTick(int tick = 0)//called during DoTick by Execute()
		{
			if(_moveTarget == null)//early skips
			{
				_moveTarget = transform.position;
				return;
			}
			else if(_moveTarget == transform.position || _isMounting)
			{
				return;
			}
			else if(_navMeshPath == null || _navMeshPath.corners.Length < 2 || _isNavPathDirty || NavSurfaceBaker._isPathsDirty)//dirty flags are resolved here, they can be set by (1) behaviors, (2) the rebaking of the navMesh, (3) turning a corner on the path
			{
				if(!RecalculateNavPath())//skip if no valid or partial path has been found
					return;
			}
			else if(_navMeshPath.status != NavMeshPathStatus.PathComplete && _navMeshPath.corners[1] - _navMeshPath.corners[0] == Vector3.forward)//Unity sets this default path (Vector3.forward) sometimes when the target destination is not on the NavMesh or close to it.
			{
				return;
			}

			float maxDistance = _moveSpeed * Time.fixedDeltaTime * 2f / GetNavAreaCost();//Why the (* 2f): NavAreaCosts below 1 give a warning, hence I double the costs in inspector and half them here.
			float walkedDistance = 0f;
			float nextCornerDistance;

			int i = 1;
			for(; i < _navMeshPath.corners.Length && walkedDistance < maxDistance; i++)//moves the pawn by maxDistance towards the next corners, even around them
			{
				nextCornerDistance = Vector3.Distance(transform.position, _navMeshPath.corners[i]);
				Vector3 moveVec;
				float tempDistance;
				tempDistance = Mathf.Min(nextCornerDistance, maxDistance - walkedDistance);
				moveVec = Vector3.MoveTowards(transform.position, _navMeshPath.corners[i], tempDistance);
				transform.position = moveVec;
				walkedDistance += tempDistance;
			}

			if(2 < i)//<=> pawn has moved over a corner
			{
				_isNavPathDirty = true;
				//RecalculateNavPath();//could be solved more elegantly performancewise, but not without copying the _navMeshPath.corners array and doing admin myself
			}

			if(i - 1 < _navMeshPath.corners.Length)
				transform.LookAt(new Vector3(_navMeshPath.corners[i - 1].x, transform.position.y, _navMeshPath.corners[i - 1].z));
		}

		public void SetMoveTarget(Vector3 newTarget)//call this from the behaviors
		{
			if(_moveTarget != newTarget)
			{
				_moveTarget = newTarget;
				_isNavPathDirty = true;
				//RecalculateNavPath();
			}
		}

		private bool RecalculateNavPath()
		{
			if(NavMesh.CalculatePath(transform.position, _moveTarget, NavSurfaceBaker._navMask, _navMeshPath))
			{
				_isNavPathDirty = false;
				return true;
			}
			else
			{
				Debug.LogWarning("Pawn " + _id + " failed to calculate NavPath.");
				return false;
			}
		}

		public void ShowNavPath()
		{
			_lineRenderer.positionCount = _navMeshPath.corners.Length;
			_lineRenderer.SetPositions(_navMeshPath.corners);
		}

		private float GetNavAreaCost()
		{
			NavMeshHit navMeshHit = new NavMeshHit();

			if(NavMesh.SamplePosition(transform.position, out navMeshHit, 0.2f, NavSurfaceBaker._navMask))
			{
				/*
				//Debug version
				int index = IndexFromMask(navMeshHit.mask);
				string debugString = "Area: " + index;
				float areaCost = NavMesh.GetAreaCost(index);
				debugString += " AreaCost: " + areaCost;
				Debug.Log(debugString);
				return areaCost;
				*/
				//Short version
				return NavMesh.GetAreaCost(IndexFromMask(navMeshHit.mask));
			}
			else
			{
				Debug.LogWarning("Pawn " + _id + " could not sample NavAreaCost. Defaulting to 1.");
				return 1f;
			}
		}

		private void SetNavPathClean(int tick) => _isNavPathDirty = false;

		private int IndexFromMask(int mask)
		{
			for(int i = 0; i < 32; ++i)
			{
				if((1 << i) == mask)
					return i;
			}
			return -1;
		}
		#endregion

		#region Physics
		[SerializeField] [Tooltip("Which layers should be used when checking for close objects with CheckOverloadSphere()?")] private LayerMask _overlapSphereLayerMask;
		private void CheckOverlapSphere()
		{
			ClearLists();

			Collider[] colliders = Physics.OverlapSphere(transform.position, 10f);//TODO: adjust sphere radius

			foreach(Collider c in colliders)
			{
				switch(c.tag)
				{
					case "Pawn":
						Pawn pawn = c.GetComponent<Pawn>();
						if(pawn)
							_closePawns.Add(pawn);
						continue;
					case "Cover":
						Cover cover = c.GetComponent<Cover>();
						if(cover)
						{
							foreach(CoverSlot slot in cover._coverSlots)
							{
								_closeCoverSlots.Add(slot);
							}
						}
						continue;
					default:
						continue;
				}
			}
		}

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
				{
					foreach(CoverSlot slot in temp._coverSlots)
					{
						_closeCoverSlots.Add(slot);
					}
				}
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
				if(temp)
				{
					foreach(CoverSlot slot in temp._coverSlots)
					{
						_closeCoverSlots.Add(slot);
					}
				}
			}
		}
		#endregion

		#region Interfaces
		private TextMeshProUGUI[] _panelDetails;
		public void InitialiseUnitPanel()
		{
			UnitScreenController.s_instance.AddUnitInfoPanel(transform, "Team: " + _team, "Health: " + _health, "Morale: " + _morale, ref _panelDetails);
		}

		public void UpdateUnitPanelInfo()
		{
			if(_panelDetails != null && 3 <= _panelDetails.Length)
			{
				_panelDetails[0].text = "Team: " + _team;
				_panelDetails[1].text = "Health: " + _health;
				_panelDetails[2].text = "Morale: " + _morale;
			}
		}
		#endregion

		#region Gizmos
		private void OnDrawGizmos()
		{
			//Gizmos.color = Color.blue;
			//Gizmos.DrawLine(transform.position, _navMeshPath.corners[_navMeshPath.corners.Length - 1]);//done with a LineRenderer up top
		}
		#endregion

		#region Spawning/Despawning
		private void ClearLists()
		{
			_closePawns.Clear();
			_closeCoverSlots.Clear();
		}

		/// <summary>
		/// Resets a pawn to factory setting.
		/// </summary>
		/// <param name="pawn">The pawn to be reset.</param>
		/// <param name="team"></param>
		private static void ResetToDefault(Pawn pawn, int team)
		{
			pawn._team = team;//not needed if object pools are per player
			pawn._health = pawn._maxHealth;
			pawn._ammo = pawn._maxAmmo;
			pawn._morale = pawn._maxMorale;
			pawn._resources = 0;
			pawn._isNavPathDirty = true;
			pawn._isAttacking = false;
			//pawn._moveSpeed = 3.6111111f;
			pawn._navMeshPath = null;
			pawn._morale = pawn._maxMorale;

			pawn.ClearLists();

			//pawn._moveTarget = Vector3.forward;
			pawn._mountSlot = null;
		}

		public static void Spawn(ObjectType pawnType, Vector3 spawnPoint, int team)
		{
			Pawn newPawn = (Pawn)ObjectPool.s_objectPools[GlobalVariables.s_instance._prefabs[(int)pawnType]].GetNextObject();
			ResetToDefault(newPawn, team);
			newPawn.transform.position = spawnPoint;
			newPawn._moveTarget = spawnPoint;
		}

		public static ObjectType RandomPawnType()
		{
			int randomNumber = UnityEngine.Random.Range(0, 3);

			switch(randomNumber)
			{
				case 0:
					return ObjectType.PAWN_HEALER;
				case 1:
					return ObjectType.PAWN_PIONEER;
				case 2:
					return ObjectType.PAWN_WARRIOR;
				default:
					return ObjectType.PAWN_WARRIOR;
			}
		}

		private void OnEnable()
		{
			TickHandler.s_SetUp += SetNavPathClean;
			TickHandler.s_AIEvaluate += Evaluate;
			TickHandler.s_DoTick += Execute;
			TickHandler.s_GatherValues += WriteToGameState;
		}

		private void OnDisable()
		{
			TickHandler.s_SetUp -= SetNavPathClean;
			TickHandler.s_AIEvaluate -= Evaluate;
			TickHandler.s_DoTick -= Execute;
			TickHandler.s_GatherValues -= WriteToGameState;

			if(_isMounting)
				Behavior_Mount.s_instance.RemoveFromTargetDict(this);//also nulls _mountSlot
		}
		#endregion
	}
}