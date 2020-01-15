using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class FlagPole : MonoBehaviour, INetElement
	{
		public IRefHolder _myRefHolder
		{
			get
			{
				if(null == _refHolderBackingField)
					_refHolderBackingField = GetComponentInParent<IRefHolder>();

				return _refHolderBackingField;
			}
		}

		private IRefHolder _refHolderBackingField;

		[SerializeField] private FlagHolder _myFlagHolder;

		private class State
		{
			public Vector3 _position;
			public float _angle;
		}

		public static float _radius = 2f;

		public int _id { get; set; }
		public int _team
		{
			get
			{
				if(null != _myRefHolder)
					return _myRefHolder._team;
				else
					return 0;
			}
		}
		public float _score = 1f;
		public int _maxPawns = 10;

		//targets and target lists
		public List<Pawn> _friendlyPawns = new List<Pawn>();//ONLY PAWNS OF MY TEAM!
		public List<Pawn> _activePawns//this is an accessor to the _closePawns List, ensuring I don't have to write this every time I want to use the list.
		{
			get
			{
				List<Pawn> offPawns = new List<Pawn>();

				foreach(var it in _friendlyPawns)
				{
					if(!it.gameObject.activeSelf)
						offPawns.Add(it);
				}

				foreach(var it in offPawns)
				{
					_friendlyPawns.Remove(it);
				}

				return _friendlyPawns;
			}
		}
		private State _lastState = new State();
		private State _nextState = new State();

		#region Monobehaviour
		private void Awake()
		{
#if !UNITY_SERVER
			TickHandler.s_DoInput += ExtractFromGameState;
#endif
		}

		void Start()
		{

		}

		void Update()
		{
#if !UNITY_SERVER
			//VisualizeLerpedStates();
#endif
		}

		private void VisualizeLerpedStates()
		{
			/*
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

			_myFlagHolder.transform.position = Vector3.Lerp(_lastState._position, _nextState._position, lerpFactor);
			_myFlagHolder.transform.eulerAngles = new Vector3(0f, Mathf.LerpAngle(_lastState._angle, _nextState._angle, lerpFactor), 0f);
			*/
		}
		#endregion

		private void Calculate(int tick = 0)
		{
			CheckOverlapSphere();
			_score = 1f - Mathf.Clamp((float)_activePawns.Count / _maxPawns, 0f, 1f);
		}

		[SerializeField] [Tooltip("Which layers should be used when checking for close objects with CheckOverloadSphere()?")] private LayerMask _overlapSphereLayerMask;
		private void CheckOverlapSphere()
		{
			ClearLists();

			Collider[] colliders = Physics.OverlapSphere(transform.position, _radius, _overlapSphereLayerMask);//TODO: adjust sphere radius

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
						if(null != pawn && _team == pawn._team)
							_friendlyPawns.Add(pawn);
						continue;
					default:
						//Debug.Log("it's a sad default: " + c.tag);
						continue;
				}
			}
		}

		private void ClearLists() => _friendlyPawns.Clear();

		public static void Spawn(Vector3 spawnPoint, int team)
		{
			FlagPole newFlagPole = (FlagPole)ObjectPool.s_objectPools[GlobalVariables.s_instance._prefabs[(int)ObjectType.FLAGPOLE]].GetNextObject(team);
			newFlagPole.transform.position = spawnPoint;
			//newFlagPole._team = team;
			newFlagPole.ClearLists();

			if(null != JobCenter.s_flagPoles[team] && !JobCenter.s_flagPoles[team].Contains(newFlagPole))
				JobCenter.s_flagPoles[team].Add(newFlagPole);
		}

		public void Init()
		{
			ClearLists();

			//if(null != JobCenter.s_flagPoles[_team] && !JobCenter.s_flagPoles[_team].Contains(this))
				//JobCenter.s_flagPoles[_team].Add(this);
		}

		#region Interfaces
		#region INetElement
		public void ExtractFromGameState(int tick)//if CLIENT: an doinput hängen
		{
			/*
			if(null != _nextState)
				_lastState = _nextState;

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
					if(!gameObject.activeSelf)
						gameObject.SetActive(true);
				}
			}
			{
				GSC.type temp = TickHandler.s_interfaceGameState.GetType(_id);

				if(null != temp)
				{
					if(_myRefHolder != null)
						_myRefHolder._team = temp._team;
				}
			}
			{
				GSC.transform temp = TickHandler.s_interfaceGameState.GetTransform(_id);

				if(null != temp)
				{
					//transform.position = temp._position;
					_nextState._position = temp._position;
					_nextState._angle = temp._angle;
				}
			}
			#endregion
			*/
		}

		public void WriteToGameState(int tick)
		{
			/*
			{
				Arguments temp = new Arguments();

				if(isActiveAndEnabled)
					temp |= Arguments.ENABLED;

				TickHandler.s_interfaceGameState._args.Add(new GSC.arg { _id = _id, _arguments = temp });

				if(!isActiveAndEnabled)
					return;//if cover is disabled, no other info is relevant
			}

			TickHandler.s_interfaceGameState._types.Add(new GSC.type { _id = _id, _type = 0, _team = (byte)_team });
			TickHandler.s_interfaceGameState._transforms.Add(new GSC.transform { _id = _id, _position = transform.position, _angle = transform.eulerAngles.y });
			*/
		}
		#endregion
		#endregion

		private void OnEnable()
		{
#if UNITY_SERVER
			Init();

			TickHandler.s_LateCalc += Calculate;
			//TickHandler.s_GatherValues += WriteToGameState;
#endif
		}

		private void OnDisable()
		{
#if UNITY_SERVER
			TickHandler.s_LateCalc -= Calculate;
			//TickHandler.s_GatherValues -= WriteToGameState;
#endif
			if(null != JobCenter.s_flagPoles[_team])
					if(JobCenter.s_flagPoles[_team].Contains(this))
						JobCenter.s_flagPoles[_team].Remove(this);
		}
		
		private void OnDestroy()
		{
#if !UNITY_SERVER
			TickHandler.s_DoInput -= ExtractFromGameState;
#endif
		}
	}
}