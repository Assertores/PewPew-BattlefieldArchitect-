using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using TMPro;

namespace PPBA
{
	public enum Behaviors : byte { IDLE, SHOOT, THROWGRENADE, GOTOFLAG, GOTOBORDER, CONQUERBUILDING, STAYINCOVER, GOTOCOVER, GOTOHEAL, FLEE, GETSUPPLIES, BRINGSUPPLIES, BUILD, DECONSTRUCT, GETAMMO, MOUNT, FOLLOW, DIE, WINCHEER, GOANYWHERE, SHOOTATBUILDING };

	[RequireComponent(typeof(LineRenderer))]
	public class Pawn : MonoBehaviour, IPanelInfo, INetElement
	{
		protected class State
		{
			public Arguments _arguments;
			public Vector3 _position;
			public float _angle; //in degrees
			public float _health;
			public float _ammo;
			public float _morale;
			public int _supplies;
			public Vector3[] _navPathCorners;
			public Behaviors _behavior;
			public PawnAnimations _animation;
		}

		#region Variables
		//public
		public static List<Pawn> _pawns = new List<Pawn>();

		public int _id { get; set; }
		public Arguments _arguments = new Arguments();
		[SerializeField] public ObjectType _pawnType;
		private int _teamBackingField;
		[SerializeField]
		public int _team
		{
			get => _teamBackingField;
			set
			{
				if(value != _teamBackingField)
					SetMaterialColor(value);

				_teamBackingField = value;
			}
		}

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

		[SerializeField] public int _supplies { get => _suppliesBackingField; set => _suppliesBackingField = Mathf.Clamp(value, 0, _maxSupplies); } //resources carried
		[SerializeField] public int _maxSupplies = 10;

		[HideInInspector] public bool _isNavPathDirty = true;//refresh every tick

		public PawnAnimations _currentAnimation = PawnAnimations.IDLE;

		//protected
		[SerializeField] [Range(1f, 10f)] private float _moveSpeed = 1f;
		[SerializeField] protected float _healthBackingField = 100;
		[SerializeField] protected int _ammoBackingField = 100;
		[SerializeField] protected int _suppliesBackingField = 0;
		[SerializeField] protected float _moraleBackingField = 100;
		[SerializeField] [Tooltip("Health regeneration per tick.")] protected float _healthRegen = 1f;
		[SerializeField] [Tooltip("Morale regeneration per tick.")] protected float _moraleRegen = 1f;
		[SerializeField] int TerritoryMapId;
		#endregion

		#region References
		//GameStates
		protected State _lastState;
		protected State _nextState;
		//Behaviors
		[SerializeField] protected Behaviors[] e_behaviors;
		[SerializeField] protected float[] _behaviorMultipliers;
		[SerializeField] protected Behavior[] _behaviors;
		public Behavior _lastBehavior = Behavior_Idle.s_instance;
		[SerializeField] [Tooltip("Displays last calculated behavior-scores.\nNo reason to change these.")] protected float[] _behaviorScores;
		public Behaviors _clientBehavior = Behaviors.IDLE;

		//Components
		public PawnAnimationController _animationController;
		public NavMeshPath _navMeshPath;
		public Vector3[] _clientNavPathCorners;
		private LineRenderer _lineRenderer;//displays path
		public ShootLineController _shootLineController;
		[SerializeField] private HealthBarController _healthBarController;
		[SerializeField] private Material _material;
		[SerializeField] private Renderer _myRenderer;
		private MaterialPropertyBlock _PropertyBlock;
		private PawnBoomboxController _myBoombox;

		//targets and target lists
		public List<Pawn> _closePawns = new List<Pawn>();
		public List<Pawn> _activePawns//this is an accessor to the _closePawns List, ensuring I don't have to write this every time I want to use the list.
		{
			get
			{
				List<Pawn> offPawns = new List<Pawn>();

				foreach(var it in _closePawns)
				{
					if(!it.gameObject.activeSelf)
						offPawns.Add(it);
				}

				foreach(var it in offPawns)
				{
					_closePawns.Remove(it);
				}

				return _closePawns;
			}
		}
		public List<CoverSlot> _closeCoverSlots = new List<CoverSlot>();
		public List<CoverSlot> _activeCoverSlots
		{
			get
			{
				List<CoverSlot> offCoverSlots = new List<CoverSlot>();

				foreach(var it in _closeCoverSlots)
				{
					if(!it.gameObject.activeSelf)
						offCoverSlots.Add(it);
				}

				foreach(var it in offCoverSlots)
				{
					_closeCoverSlots.Remove(it);
				}

				return _closeCoverSlots;
			}
		}
		public List<IDestroyableBuilding> _closeBuildings = new List<IDestroyableBuilding>();
		public List<IDestroyableBuilding> _activeBuildings
		{
			get
			{
				List<IDestroyableBuilding> offBuildings = new List<IDestroyableBuilding>();

				foreach(var it in _closeBuildings)
				{
					if(!it.GetTransform().gameObject.activeSelf)
						offBuildings.Add(it);
				}

				foreach(var it in offBuildings)
				{
					_closeBuildings.Remove(it);
				}

				return _closeBuildings;
			}
		}
		public Vector3 _moveTarget;//let the behaviors set this
		public Vector3 _behaviorTarget = Vector3.zero;
		public MountSlot _mountSlot = null;
		public bool _isMounting => _mountSlot != null && _mountSlot.isActiveAndEnabled;
		public Vector3 _borderData = Vector3.zero;

		[SerializeField] private Material _ringMaterial;
		[SerializeField] private MeshRenderer _ringRenderer;
		#endregion

		#region MonoBehaviour
		private void Awake()
		{
			//Get references
			_navMeshPath = new NavMeshPath();
			_lineRenderer = GetComponent<LineRenderer>();
			_healthBarController = GetComponentInChildren<HealthBarController>();
			_shootLineController = GetComponentInChildren<ShootLineController>();
			_animationController = transform.GetChild(0).GetComponent<PawnAnimationController>();
			_myBoombox = GetComponentInChildren<PawnBoomboxController>();

			//Initialisation
			InitiateBehaviors();

			//Safeguard against a too short _behaviorMultipliers array.
			if(_behaviorMultipliers.Length < e_behaviors.Length)
			{
				float[] arr = new float[e_behaviors.Length];

				int i = 0;
				for(; i < _behaviorMultipliers.Length; i++)
				{
					arr[i] = _behaviorMultipliers[i];
				}

				for(; i < e_behaviors.Length; i++)
				{
					arr[i] = 1f;
				}

				_behaviorMultipliers = arr;
			}

#if !UNITY_SERVER
			TickHandler.s_DoInput += ExtractFromGameState;
#else
			TickHandler.s_GatherValues += WriteToGameState;
#endif
		}

		private void Start()
		{
			_lineRenderer.enabled = false;
		}

		private void Update()
		{
#if !UNITY_SERVER
			VisualizeLerpedStates();
#endif
			_animationController.SetAnimatorBools(_currentAnimation);
			//_healthBarController.SetBars(_health / _maxHealth, _morale / _maxMorale, (float)_ammo / _maxAmmo);
			_healthBarController.SetBars(_health / _maxHealth, (float)_supplies / _maxSupplies, (float)_ammo / _maxAmmo);
			ShowNavPath();
		}
		#endregion

		#region Tick
		protected void Evaluate(int tick = 0)   //uses behavior-scores to evaluate behaviors
		{
			CheckOverlapSphere();
			GetBorderData();

			if(_isMounting)
			{
				float tempScore = Behavior_StayInCover.s_instance.Calculate(this);

				if(tempScore < 0.1f)
					_mountSlot.GetOut(this);
				else
					return;
			}

			if(null != _behaviors)
			{
				for(int i = 0; i < _behaviors.Length; i++)
				{
					_behaviorScores[i] = _behaviors[i].Calculate(this);

					if(i < _behaviorMultipliers.Length)
						_behaviorScores[i] *= _behaviorMultipliers[i];

					//Debug.Log("Pawn: " + this.name + " -- Behavior: " + _behaviors[i]._name + " got score " + _behaviorScores[i]);
				}
			}
		}

		protected void Execute(int tick = 0)   //calls the execution on the most appropriate behavior
		{
			int bestBehavior = 0;
			float bestScore = 0;

			if(_health <= 0)//early skip: death
			{
				Behavior_Die.s_instance.Execute(this);
				return;
			}

			Regenerate();

			if(_isMounting)//early skip: mounting
			{
				Behavior_StayInCover.s_instance.Execute(this);
				return;
			}
			else if(0 < _behaviors.Length)
			{
				for(int i = 0; i < _behaviorScores.Length; i++)//determines best behavior
				{
					if(bestScore < _behaviorScores[i])
					{
						bestBehavior = i;
						bestScore = _behaviorScores[i];
					}
				}

				if(_lastBehavior != null && _lastBehavior != _behaviors[bestBehavior])//on behavior change
				{
					_lastBehavior.RemoveFromTargetDict(this);//remove from lastBehaviors targetList

					//change animation
				}

				_lastBehavior = _behaviors[bestBehavior];
				_lastBehavior.Execute(this);
			}

			NavTick();
		}

		public void WriteToGameState(int tick)//SERVER
		{
			{
				Arguments temp = new Arguments();

				if(isActiveAndEnabled)
					temp |= Arguments.ENABLED;

				if(_arguments.HasFlag(Arguments.TRIGGERBEHAVIOUR))
					temp |= Arguments.TRIGGERBEHAVIOUR;

				TickHandler.s_interfaceGameState.Add(new GSC.arg { _id = _id, _arguments = temp });

				if(!isActiveAndEnabled)
					return;//if pawn is disabled, no other info is relevant
			}

			TickHandler.s_interfaceGameState.Add(new GSC.type { _id = _id, _type = 0, _team = (byte)_team });
			TickHandler.s_interfaceGameState.Add(new GSC.transform { _id = _id, _position = transform.position, _angle = transform.eulerAngles.y });
			TickHandler.s_interfaceGameState.Add(new GSC.health { _id = _id, _health = _health, _morale = _morale });
			TickHandler.s_interfaceGameState.Add(new GSC.ammo { _id = _id, _bullets = _ammo });
			TickHandler.s_interfaceGameState.Add(new GSC.resource { _id = _id, _resources = _supplies });
			if(_lastBehavior != null)
				TickHandler.s_interfaceGameState.Add(new GSC.behavior { _id = _id, _behavior = _lastBehavior._name, _target = _lastBehavior.GetTargetID(this) });
			if(_navMeshPath != null)
				TickHandler.s_interfaceGameState.Add(new GSC.path { _id = _id, _path = _navMeshPath.corners });
			TickHandler.s_interfaceGameState.Add(new GSC.animation { _id = _id, _animation = _currentAnimation });
		}

		public void ExtractFromGameState(int tick)//if CLIENT: an doinput hängen
		{
			if(null != _nextState)
				_lastState = _nextState;

			if(TickHandler.s_interfaceGameState._isNULLGameState)
				return;

			_nextState = new State();

			#region Writing into _nextState from s_interfaceGameState
			{
				GSC.arg temp = TickHandler.s_interfaceGameState.GetArg(_id);

				if(null == temp || !temp._arguments.HasFlag(Arguments.ENABLED))
				{
					if(gameObject.activeSelf)
						gameObject.SetActive(false);
					return;
				}
				else
				{
					_nextState._arguments = temp._arguments;

					if(!gameObject.activeSelf)
					{
						gameObject.SetActive(true);
						//_myBoombox.PlaySpawn();
						ResetToDefault(this, 0);
					}
				}

				if(temp._arguments.HasFlag(Arguments.TRIGGERBEHAVIOUR))
				{
					//_nextState._arguments |= Arguments.TRIGGERBEHAVIOUR;
				}
			}
			{
				GSC.type temp = TickHandler.s_interfaceGameState.GetType(_id);

				if(null != temp)
				{
					if(_team != temp._team)
						SetMaterialColor(_team);

					_team = temp._team;
				}
			}
			{
				GSC.transform temp = TickHandler.s_interfaceGameState.GetTransform(_id);

				if(null != temp)
				{
					_nextState._position = temp._position;
					_nextState._angle = temp._angle;
				}
			}
			{
				GSC.health temp = TickHandler.s_interfaceGameState.GetHealth(_id);
				if(null != temp)
				{
					_nextState._health = temp._health;
					_nextState._morale = temp._morale;

					if(_nextState._health < _health && 0 != _nextState._health)//damage sound. not played on death, to avoid doubling with the command in OnDisable()
						MovableSpeakerController.PlaySoundAtSpot(AudioWarehouse.s_instance.Clip(UnityEngine.Random.Range(0f, 1f) < 0.5f ? ClipsPawn.UNIT_HIT_SHOT_01 : ClipsPawn.UNIT_HIT_SHOT_02), transform.position);
				}
			}
			{
				GSC.ammo temp = TickHandler.s_interfaceGameState.GetAmmo(_id);

				if(null != temp)
				{
					_nextState._ammo = temp._bullets;
				}
			}
			{
				GSC.resource temp = TickHandler.s_interfaceGameState.GetResource(_id);

				if(null != temp)
				{
					_nextState._supplies = temp._resources;
				}
			}
			{
				GSC.behavior temp = TickHandler.s_interfaceGameState.GetBehavior(_id);

				if(null != temp)
				{
					_nextState._behavior = temp._behavior;
					_clientBehavior = temp._behavior;

					GSC.transform foo = TickHandler.s_interfaceGameState.GetTransform(temp._target);

					if(null != foo && null != foo._position)
					{
						_behaviorTarget = foo._position;
					}
				}
			}
			{
				GSC.path temp = TickHandler.s_interfaceGameState.GetPath(_id);

				if(null != temp)
				{
					_nextState._navPathCorners = temp._path;
				}
			}
			{
				GSC.animation temp = TickHandler.s_interfaceGameState.GetAnimation(_id);

				if(null != temp)
				{
					_nextState._animation = temp._animation;
				}
			}
			#endregion
		}

		protected void VisualizeLerpedStates()
		{
			float lerpFactor;

			if(null == _lastState || null == _nextState)
			{
				if(null != _nextState)
					lerpFactor = 1f;
				else if(null != _lastState)
					lerpFactor = 0f;
				else
					return;
			}
			else
				lerpFactor = (Time.time - TickHandler.s_currentTickTime) / Time.fixedDeltaTime;

			_arguments = _nextState._arguments;

			transform.position = Vector3.Lerp(_lastState._position, _nextState._position, lerpFactor);
			transform.eulerAngles = new Vector3(0f, Mathf.LerpAngle(_lastState._angle, _nextState._angle, lerpFactor), 0f);
			_health = Mathf.Lerp(_lastState._health, _nextState._health, lerpFactor);
			_ammo = (int)Mathf.Lerp(_lastState._ammo, _nextState._ammo, lerpFactor);
			_morale = Mathf.Lerp(_lastState._morale, _nextState._morale, lerpFactor);
			_supplies = (int)Mathf.Lerp(_lastState._supplies, _nextState._supplies, lerpFactor);
			_clientNavPathCorners = _nextState._navPathCorners;
			_clientBehavior = _nextState._behavior;
			_currentAnimation = _nextState._animation;

			if(_arguments.HasFlag(Arguments.TRIGGERBEHAVIOUR))
				SoundSwitch();
		}

		private void SoundSwitch()
		{
			switch(_clientBehavior)
			{
				case Behaviors.IDLE:
					break;
				case Behaviors.SHOOT:
					_shootLineController.SetShootLine(_shootLineController.transform.position, _behaviorTarget);
					break;
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
				case Behaviors.GETSUPPLIES:
					break;
				case Behaviors.BRINGSUPPLIES:
					break;
				case Behaviors.BUILD:
					_myBoombox.PlayBehavior(ClipsPawn.BUILD_REPAIR_01);
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
				case Behaviors.SHOOTATBUILDING:
					//_shootLineController.SetShootLine(_shootLineController.transform.position, _behaviorTarget);
					_shootLineController.SetShootLine(_shootLineController.transform.position, transform.position + (transform.forward * 0.75f * _attackDistance));
					break;
				default:
					_shootLineController.SetShootLine(_shootLineController.transform.position, _behaviorTarget);
					break;
			}
		}
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
				case Behaviors.SHOOTATBUILDING:
					return Behavior_ShootAtBuilding.s_instance;
				case Behaviors.THROWGRENADE:
					break;
				case Behaviors.GOTOFLAG:
					return Behavior_GoToFlag.s_instance;
				case Behaviors.GOTOBORDER:
					return Behavior_GoToBorder.s_instance;
				case Behaviors.CONQUERBUILDING:
					break;
				case Behaviors.STAYINCOVER:
					return Behavior_StayInCover.s_instance;
				case Behaviors.GOTOCOVER:
					break;
				case Behaviors.GOTOHEAL:
					return Behavior_GoToHeal.s_instance;
				case Behaviors.FLEE:
					break;
				case Behaviors.GETSUPPLIES:
					return Behavior_GetSupplies.s_instance;
				case Behaviors.BRINGSUPPLIES:
					return Behavior_BringSupplies.s_instance;
				case Behaviors.BUILD:
					return Behavior_Build.s_instance;
				case Behaviors.DECONSTRUCT:
					break;
				case Behaviors.GETAMMO:
					return Behavior_GetAmmo.s_instance;
				case Behaviors.MOUNT:
					return Behavior_Mount.s_instance;
				case Behaviors.FOLLOW:
					break;
				case Behaviors.DIE:
					return Behavior_Die.s_instance;
				case Behaviors.WINCHEER:
					break;
				case Behaviors.GOANYWHERE:
					return Behavior_GoAnywhere.s_instance;
				default:
#if DB_AI
					Debug.LogWarning("GetBehavior switch defaulted. Couldn't get the desired behavior.");
#endif
					return Behavior_GoAnywhere.s_instance;
			}

			return Behavior_GoAnywhere.s_instance;
		}

		public Behaviors GetBehaviorsEnum(Behavior behavior) => behavior._name;
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
			if(0f < _health)
			{
				_health += _healthRegen;
				_morale += _moraleRegen;
			}
		}
		#endregion

		#region Navigation
		public void NavTick(int tick = 0)//called during DoTick by Execute()
		{
			Vector3 originalPos = transform.position;

			if(null == _navMeshPath)
				_navMeshPath = new NavMeshPath();

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
			float walkedDistance = 0.5f;
			float nextCornerDistance;

			int i = 1;
			for(; i < _navMeshPath.corners.Length && walkedDistance < maxDistance; i++)//moves the pawn by maxDistance towards the next corners, even around them
			{
				walkedDistance -= 0.5f;
				nextCornerDistance = Vector3.Distance(transform.position, _navMeshPath.corners[i]);
				float tempDistance = Mathf.Min(nextCornerDistance, maxDistance - walkedDistance);
				Vector3 moveVec = Vector3.MoveTowards(transform.position, _navMeshPath.corners[i], tempDistance);
				transform.position = moveVec;
				walkedDistance += tempDistance;
			}

			if(2 < i)//<=> pawn has moved over a corner
				_isNavPathDirty = true;

			if(i - 1 < _navMeshPath.corners.Length)
				transform.LookAt(new Vector3(_navMeshPath.corners[i - 1].x, transform.position.y, _navMeshPath.corners[i - 1].z));

			//if(Vector3.Magnitude(originalPos - _moveTarget) < 1f || Vector3.Magnitude(transform.position - originalPos) < 1f)
				//transform.position = Behavior_GoAnywhere.s_instance.GetRandomPoint(this);
		}

		public void SetMoveTarget(Vector3 newTarget)//call this from the behaviors
		{
			if(_moveTarget != newTarget)
			{
				_moveTarget = newTarget;
				_isNavPathDirty = true;
			}
		}

		private bool RecalculateNavPath()
		{
			if(null == _moveTarget || null == _navMeshPath)
			{
#if DB_AI || DB_PF
				Debug.LogWarning("Pawn is missing a _moveTarget or a _navMeshPath");
#endif
				return false;
			}

			if(NavMesh.CalculatePath(transform.position, _moveTarget, NavSurfaceBaker._navMask, _navMeshPath))
			{
				_isNavPathDirty = false;
				return true;
			}
			else
			{
#if DB_AI || DB_PF
				Debug.LogWarning("Pawn " + _id + " failed to calculate NavPath.");
#endif
				return false;
			}
		}

		public void ShowNavPath()
		{
#if UNITY_SERVER
			if(null != _navMeshPath?.corners)
			{
				_lineRenderer.positionCount = _navMeshPath.corners.Length;
				_lineRenderer.SetPositions(_navMeshPath.corners);
			}
#else
			if(null != _clientNavPathCorners)
			{
				_lineRenderer.positionCount = _clientNavPathCorners.Length;
				_lineRenderer.SetPositions(_clientNavPathCorners);
			}
#endif
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
#if DB_AI || DB_PF
				Debug.LogWarning("Pawn " + _id + " could not sample NavAreaCost. Defaulting to 1.");
#endif
				return 1f;
			}
		}

		private void SetNavPathClean(int tick = 0) => _isNavPathDirty = false;
		private void ClearFlags(int tick = 0)
		{
			SetNavPathClean();
			_arguments = new Arguments();
		}

		private int IndexFromMask(int mask)
		{
			for(int i = 0; i < 32; ++i)
			{
				if((1 << i) == mask)
					return i;
			}
			return -1;
		}

		public void GetBorderData(int tick = 0) => _borderData = HeatMapHandler.s_instance.BorderValues(transform.position);
		#endregion

		#region Physics
		[SerializeField] [Tooltip("Which layers should be used when checking for close objects with CheckOverloadSphere()?")] private LayerMask _overlapSphereLayerMask;
		private void CheckOverlapSphere()
		{
			ClearLists();

			Collider[] colliders = Physics.OverlapSphere(transform.position, 50f, _overlapSphereLayerMask);//TODO: adjust sphere radius

			//Debug.Log("I found " + colliders.Length.ToString() + " colliders.");

			foreach(Collider c in colliders)
			{
				//Debug.Log("Found a collider named: " + c.name + " on GameObject " + c.gameObject.name + ".");

				switch(c.tag)
				{
					case StringCollection.PAWN:
						if(c.transform.parent.tag == StringCollection.PAWN)//exit condition: Break if I'm in a child of the Pawn, to reduce use of GetComponent()
							continue;

						Pawn pawn = c.GetComponent<Pawn>();
						if(null != pawn && this != pawn)
							_closePawns.Add(pawn);
						continue;
					case StringCollection.COVER:
						if(c.transform.parent.tag == StringCollection.COVER)//exit condition: Break if I'm in a child of the Cover, to reduce use of GetComponent()
							continue;

						Cover cover = c.GetComponent<Cover>();
						if(null != cover)
						{
							_closeBuildings.Add(cover);

							foreach(CoverSlot slot in cover._coverSlots)
							{
								_closeCoverSlots.Add(slot);
							}
						}
						continue;
					case StringCollection.DEPOT:
						ResourceDepot depot = c.GetComponentInChildren<ResourceDepot>();
						if(null != depot)
						{
							_closeBuildings.Add(depot);
						}
						continue;
					case StringCollection.FLAGPOLE:
						continue;
					case StringCollection.HQ:
						HeadQuarter hq = c.transform.GetChild(2)?.GetComponent<HeadQuarter>();
						if(null != hq)
						{
							_closeBuildings.Add(hq._resourceDepot);
						}
						continue;
					case StringCollection.MEDICAMP:
						MediCamp mediCamp = c.transform.GetChild(2)?.GetComponent<MediCamp>();
						if(null != mediCamp)
						{
							_closeBuildings.Add(mediCamp);
						}
						continue;
					case StringCollection.REFINERY:
						IDestroyableBuilding refinery = c.transform.GetChild(2)?.GetComponent<IDestroyableBuilding>();
						if(null != refinery)
						{
							_closeBuildings.Add(refinery);
						}
						continue;
					case StringCollection.WALL:
						IDestroyableBuilding wall = c.transform.GetChild(2)?.GetComponent<IDestroyableBuilding>();
						if(null != wall)
						{
							_closeBuildings.Add(wall);
						}
						continue;
					default:
						//Debug.Log("it's a sad default: " + c.tag);
						continue;
				}
			}
		}
		#endregion

		#region Interfaces
		private TextMeshProUGUI[] _panelDetails = new TextMeshProUGUI[0];
		public void InitialiseUnitPanel()
		{
			string[] details = new string[] { "Team: " + _team, "Health: " + (int)_health, "Supplies: " + _supplies, "Ammo: " + _ammo, "Morale: " + (int)_morale };
			UnitScreenController.s_instance.AddUnitInfoPanel(transform, details, ref _panelDetails);

			if(null != _myBoombox)
				_myBoombox.PlayClick();
		}

		public void UpdateUnitPanelInfo()
		{
			if(null == _panelDetails)
				return;

			//Debug.Log("updating panel info " + _panelDetails + " with length " + _panelDetails.Length + " of pawn " + _id);

			if(_panelDetails != null && 5 <= _panelDetails.Length)
			{
				_panelDetails[0].text = "Team: " + _team;
				_panelDetails[1].text = "Health: " + (int)_health;
				_panelDetails[2].text = "Supplies: " + _supplies;
				_panelDetails[3].text = "Ammo: " + _ammo;
				_panelDetails[4].text = "Morale: " + (int)_morale;
			}
		}
		#endregion

		#region Gizmos
		private void OnDrawGizmos()
		{
			/*
			Gizmos.color = Color.blue;
			Gizmos.DrawLine(transform.position, _navMeshPath.corners[_navMeshPath.corners.Length - 1]);//done with a LineRenderer up top
			*/
		}
		#endregion

		#region Spawning/Despawning
		private void ClearLists()
		{
			_closePawns.Clear();
			_closeCoverSlots.Clear();
			_closeBuildings.Clear();
		}

		/// <summary>
		/// Resets a pawn to factory setting.
		/// </summary>
		/// <param name="pawn">The pawn to be reset.</param>
		/// <param name="team"></param>
		private static void ResetToDefault(Pawn pawn, int team)
		{
			pawn._arguments = new Arguments();
			pawn._team = team;
			pawn._health = pawn._maxHealth;
			pawn._ammo = pawn._maxAmmo;
			pawn._morale = pawn._maxMorale;
			pawn._supplies = 0;
			pawn._isNavPathDirty = true;
			//pawn._moveSpeed = 3.6111111f;
			pawn._navMeshPath = new NavMeshPath();
			pawn._morale = pawn._maxMorale;

			pawn.ClearLists();

			//pawn._moveTarget = Vector3.forward;
			pawn._mountSlot = null;

			pawn._lastState = new State();
			pawn._nextState = new State();

			pawn.SetMaterialColor(team);
		}

		public static void Spawn(ObjectType pawnType, Vector3 spawnPoint, int team)
		{
			Pawn newPawn = (Pawn)ObjectPool.s_objectPools[GlobalVariables.s_instance._prefabs[(int)pawnType]].GetNextObject(team);
			ResetToDefault(newPawn, team);
			newPawn.transform.position = spawnPoint + Vector3.forward * 5f;
			newPawn._moveTarget = spawnPoint + Vector3.forward * 10f;

#if UNITY_SERVER
			Vector2 pos = UserInputController.s_instance.GetTexturePixelPoint(newPawn.transform);
			newPawn.TerritoryMapId = HeatMapCalcRoutine.s_instance.AddSoldier(newPawn.transform, team);
			GlobalVariables.IncrementAICounter(team);
#endif
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
					return ObjectType.PAWN_HEALER;
			}
		}

		private void SetMaterialColor(int team = 0)
		{
			Color color;

			if(null != GlobalVariables.s_instance._teamColors && team < GlobalVariables.s_instance._teamColors.Length)
				color = GlobalVariables.s_instance._teamColors[team];
			else
				switch(team)
				{
					case 0:
						color = Color.green;
						break;
					case 1:
						color = Color.blue;
						break;
					case 2:
						color = Color.yellow;
						break;
					case 3:
						color = Color.red;
						break;
					default:
						color = Color.gray;
						break;
				}

			if(null == _myRenderer)
				_myRenderer = transform.GetChild(0)?.GetChild(1)?.GetComponent<Renderer>();

			//if(null == _material)
			//_material = transform.GetChild(0)?.GetChild(1)?.GetComponent<Material>();

			//if(null != _material)
			//{
			if(null != _myRenderer)
			{
				_myRenderer.GetPropertyBlock(_PropertyBlock);
				_PropertyBlock.SetColor("_Emission", color);
				_myRenderer.SetPropertyBlock(_PropertyBlock);
			}
			//}
			else
			{
#if DB_AI
				Debug.LogWarning("Pawn still couldn't get a renderer");
#endif
			}

			if(_ringRenderer != null && _ringMaterial != null)
			{
				//_ringRenderer.GetPropertyBlock(_PropertyBlock);
				//_PropertyBlock.SetColor("_Emission", color);
				//_ringRenderer.SetPropertyBlock(_PropertyBlock);
				_ringMaterial.SetColor("_EmissionColor", color);
			}
		}

		/// <summary>
		/// Returns the number of active pawns of a team.
		/// </summary>
		public static int GetActivePawnNumber(int team) => _pawns.FindAll((x) => x.isActiveAndEnabled && x._team == team).Count;//SCHNITTSTELLE RENE TODO: use

		/// <summary>
		/// Returns the number of active pawns of a team per type.
		/// </summary>
		/// <returns>(#WARRIOR, #HEALER, #PIONEER) of a team.</returns>
		public static Vector3 GetActivePawnTypes(int team)
		{
			Vector3 ticker = Vector3.zero;

			foreach(Pawn pawn in _pawns.FindAll((x) => x.isActiveAndEnabled && x._team == team))
			{
				switch(pawn._pawnType)
				{
					case ObjectType.PAWN_WARRIOR:
						ticker += Vector3.right;
						continue;
					case ObjectType.PAWN_HEALER:
						ticker += Vector3.up;
						continue;
					case ObjectType.PAWN_PIONEER:
						ticker += Vector3.forward;
						continue;
					default:
						continue;
				}
			}

			return ticker;
		}

		private void OnEnable()
		{
			_PropertyBlock = new MaterialPropertyBlock();
			SetMaterialColor(_team);

#if UNITY_SERVER
			TickHandler.s_SetUp += ClearFlags;
			TickHandler.s_AIEvaluate += Evaluate;
			TickHandler.s_DoTick += Execute;

#endif
			if(!_pawns.Contains(this))
				_pawns.Add(this);
		}

		private void OnDisable()
		{
#if UNITY_SERVER
			TickHandler.s_SetUp -= ClearFlags;
			TickHandler.s_AIEvaluate -= Evaluate;
			TickHandler.s_DoTick -= Execute;

			if(_isMounting)
				Behavior_Mount.s_instance.RemoveFromTargetDict(this);//also nulls _mountSlot

			Vector2 pos = UserInputController.s_instance.GetTexturePixelPoint(transform);
			//	TerritoryMapId = TerritoriumMapCalculate.s_instance.AddSoldier(transform, _team);
#else
			MovableSpeakerController.PlaySoundAtSpot(AudioWarehouse.s_instance.Clip(UnityEngine.Random.Range(0f, 1f) < 0.5f ? ClipsPawn.UNIT_HIT_SHOT_01 : ClipsPawn.UNIT_HIT_SHOT_02), transform.position);
#endif
			if(_pawns.Contains(this))
				_pawns.Remove(this);
		}

		private void OnDestroy()
		{
#if UNITY_SERVER
			TickHandler.s_GatherValues -= WriteToGameState;
#endif
		}
		#endregion
	}
}