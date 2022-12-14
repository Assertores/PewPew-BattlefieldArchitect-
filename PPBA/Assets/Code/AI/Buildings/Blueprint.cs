using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class Blueprint : MonoBehaviour, INetElement
	{
		protected class State
		{
			public Vector3 _position;
			public float _angle; //in degrees
								 //public float _health;
			public int _resources;
			public int _work;
		}

		#region Variables
		[SerializeField] public int _id { get; set; }
		public int _team
		{
			get
			{
				if(null == _myRefHolder)
					_myRefHolder = GetComponentInParent<IRefHolder>();

				if(null == _myRefHolder)
				{
#if DB_PO
					Debug.LogError("Blueprint couldn't find IRefHolder");
#endif
					return 0;
				}
				else
					return _myRefHolder._team;
			}
		}
		[SerializeField] public int _resources = 0;
		[SerializeField] public int _resourcesIncoming = 0;
		[SerializeField] public int _resourcesMax = 100;
		[SerializeField] public int _work = 0;
		[SerializeField] public int _workMax = 50;
		[SerializeField] public float _interactRadius = 5f;

		private bool _isFinished = false;
		//public Arguments _arguments = new Arguments();
#endregion

#region References
		private IRefHolder _myRefHolder;
		[SerializeField] public List<Pawn> _workers = new List<Pawn>();

		//GameStates
		protected State _lastState;
		protected State _nextState;

		//private
		[SerializeField] private Material _material;
		[SerializeField] private Renderer _myRenderer;
		private MaterialPropertyBlock _PropertyBlock;

#endregion

		public float _workDoable
		{
			//get => ((float)(_resources * _resourcesMax) / (float)_workMax) - (float)_work;
			get => Mathf.Clamp(((float)(_resources * _workMax)) / (float)_resourcesMax - (float)_work, 0f, _workMax);
		}

		public float _resourcesNeeded
		{
			get => (float)(_resourcesMax - _resources - _resourcesIncoming);
		}

#region Monobehavior
		void Awake()
		{
			_myRefHolder = GetComponentInParent<IRefHolder>();
#if !UNITY_SERVER
			TickHandler.s_DoInput += ExtractFromGameState;
#else
			TickHandler.s_GatherValues += WriteToGameState;
#endif
		}

		void Update()
		{
#if !UNITY_SERVER
			VisualizeLerpedStates();
#endif
		}
#endregion

#region Give & Take
		public void WorkTick()
		{
			foreach(Pawn w in _workers)
			{
				_work += 1;
			}

			if(_workMax <= _work)
				WorkIsFinished();
		}

		public void DoWork(uint amount = 1)
		{
			_work += (int)amount;

			if(_workMax <= _work)
				WorkIsFinished();
		}

		private void WorkIsFinished()
		{
			if(JobCenter.s_blueprints[_team].Contains(this))
				JobCenter.s_blueprints[_team].Remove(this);

			_material.SetFloat("_Clip", 1f);//ensure building is not dissolved

			if(null != _myRefHolder)
			{
				_myRefHolder.GetShaderProperties = UserInputController.s_instance.GetTexturePixelPoint(this.transform);
				HeatMapCalcRoutine.s_instance.AddFabric(_myRefHolder);//HAS TO DIFFER PER PREFAB
			}

			transform.parent.GetChild(2)?.gameObject.SetActive(true);//activate child with building
			transform.parent.GetChild(1)?.gameObject.SetActive(false);//deactivate child with blueprint

			_isFinished = true;
		}

		public int GiveResources(int amount)
		{
			int spaceLeft = _resourcesMax - _resources;

			if(amount <= spaceLeft)//enough space
			{
				_resources += amount;
				return amount;
			}
			else//not enough space
			{
				_resources += spaceLeft;
				return spaceLeft;
			}
		}
#endregion

#region Initialisation
		public void WriteToGameState(int tick)
		{
			{
				Arguments temp = new Arguments();

				if(isActiveAndEnabled)
					temp |= Arguments.ENABLED;

				if(_isFinished)
					temp |= Arguments.TRIGGERBEHAVIOUR;

				TickHandler.s_interfaceGameState.Add(new GSC.arg { _id = _id, _arguments = temp });
				TickHandler.s_interfaceGameState.Add(new GSC.work { _id = _id, _work = _work });

				if(!isActiveAndEnabled)
					return;//if pawn is disabled, no other info is relevant
			}

			TickHandler.s_interfaceGameState.Add(new GSC.transform { _id = _id, _position = transform.position, _angle = transform.eulerAngles.y });
			TickHandler.s_interfaceGameState.Add(new GSC.resource { _id = _id, _resources = _resources });
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

				if(null == temp)
					return;

				/*
				if(!gameObject.activeSelf && temp._arguments.HasFlag(Arguments.ENABLED))
				{
					ResetToDefault(this);
					gameObject.SetActive(true);
				}
				*/

				if(temp._arguments.HasFlag(Arguments.TRIGGERBEHAVIOUR))
				{
					_isFinished = true;
#if !UNITY_SERVER
					transform.parent.GetChild(2)?.gameObject.SetActive(true);//activate child with building
#endif
				}
				else
					_isFinished = false;
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
				GSC.resource temp = TickHandler.s_interfaceGameState.GetResource(_id);

				if(null != temp)
				{
					_nextState._resources = temp._resources;
				}
			}
			{
				GSC.work temp = TickHandler.s_interfaceGameState.GetWork(_id);
				if(null != temp)
				{
					_nextState._work = temp._work;
				}
			}
#endregion
		}

		private static void ResetToDefault(Blueprint blueprint)
		{
			blueprint._isFinished = false;
			blueprint._resources = 0;
			blueprint._work = 0;

			//blueprint.ClearLists();

			blueprint._lastState = new State();
			blueprint._nextState = new State();
		}

		public void VisualizeLerpedStates()//clientside
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

			_resources = (int)Mathf.Lerp(_lastState._resources, _nextState._resources, lerpFactor);
			_work = (int)Mathf.Lerp(_lastState._work, _nextState._work, lerpFactor);

			if(null != _material)
			{
				_myRenderer.GetPropertyBlock(_PropertyBlock);

				if(_isFinished)
					_PropertyBlock.SetFloat("_Clip", 1f);
				else
					_PropertyBlock.SetFloat("_Clip", (float)_work / _workMax);

				_myRenderer.SetPropertyBlock(_PropertyBlock);
				//	_material.SetFloat("_Clip", (float)_work / _workMax);
			}
		}

		public void SetClipFull()
		{
			if(null != _material)
			{
				_myRenderer.GetPropertyBlock(_PropertyBlock);
				_PropertyBlock.SetFloat("_Clip", 1f);
				_PropertyBlock.SetColor("_Emission", Color.green);
				_myRenderer.SetPropertyBlock(_PropertyBlock);
			}
		}

		public void SetClipEmpty()
		{
			if(null != _material)
			{
				_myRenderer.GetPropertyBlock(_PropertyBlock);
				_PropertyBlock.SetFloat("_Clip", 0f);
				_myRenderer.SetPropertyBlock(_PropertyBlock);
			}
		}


		public void CalculateIncomingSupplies(int tick = 0)
		{
			/*
			_resourcesIncoming = 0;

			foreach(Pawn pawn in Pawn._pawns)
			{
				if(pawn.enabled == false)
					continue;

				if(Behavior_BringSupplies.s_targetDictionary.ContainsKey(pawn))
				if(Behavior_BringSupplies.s_targetDictionary[pawn])
			}
			*/
		}

		private void OnEnable()
		{
			ResetToDefault(this);

			_PropertyBlock = new MaterialPropertyBlock();
			_myRefHolder = GetComponentInParent<IRefHolder>();
			_myRefHolder.GetShaderProperties = UserInputController.s_instance.GetTexturePixelPoint(this.transform);

#if UNITY_SERVER
			if(null != JobCenter.s_blueprints?[_team] && !JobCenter.s_blueprints[_team].Contains(this))
				JobCenter.s_blueprints[_team].Add(this);
#else
			//SetClipEmpty();
#endif
		}

		private void OnDisable()
		{
#if UNITY_SERVER
			if(null != JobCenter.s_blueprints?[_team] && JobCenter.s_blueprints[_team].Contains(this))
				JobCenter.s_blueprints[_team].Remove(this);
#endif
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
