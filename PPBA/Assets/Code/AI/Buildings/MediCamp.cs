using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace PPBA
{
	public class MediCamp : MonoBehaviour, INetElement, IDestroyableBuilding, IPanelInfo
	{
		[SerializeField] private MediCampHolder _myMediCampHolder;
		[HideInInspector] public BuildingBoomboxController _myBoombox;

		public int _id { get; set; }
		[SerializeField] public float _health = 1000;
		[SerializeField] private float _maxHealth = 1000;

		private class State
		{
			//public Vector3 _position;
			//public float _angle;
			public float _health;
		}

		private State _lastState = new State();
		private State _nextState = new State();

		[SerializeField] public static float _interactRadius = 1.5f;

		private int _team => _myMediCampHolder._team;

		#region Monobehaviour
		private void Awake()
		{
			_myBoombox = transform.parent.GetComponentInChildren<BuildingBoomboxController>();

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
			VisualizeLerpedStates();
#endif
		}

		private void OnEnable()
		{
#if UNITY_SERVER
			Init();

			//TickHandler.s_LateCalc += Calculate;
			TickHandler.s_GatherValues += WriteToGameState;
#endif
		}

		private void OnDisable()
		{
#if UNITY_SERVER
			//TickHandler.s_LateCalc -= Calculate;
			TickHandler.s_GatherValues -= WriteToGameState;

			if(null != JobCenter.s_mediCamp[_team] && JobCenter.s_mediCamp[_team].Contains(this))
					JobCenter.s_mediCamp[_team].Remove(this);
#endif

			gameObject.SetActive(false);
		}

		private void OnDestroy()
		{
#if !UNITY_SERVER
			TickHandler.s_DoInput -= ExtractFromGameState;
#endif
		}
		#endregion

		public void Init()
		{
			//ClearLists();
			_health = _maxHealth;

			if(null != JobCenter.s_mediCamp[_team])
				JobCenter.s_mediCamp[_team].Add(this);
		}

		#region Tick
		private void WriteToGameState(int tick = 0)
		{
			TickHandler.s_interfaceGameState.Add(new GSC.health { _id = _id, _health = _health, _morale = 0f });
		}

		private void ExtractFromGameState(int tick = 0)
		{
			if(TickHandler.s_interfaceGameState._isNULLGameState)
				return;

			if(null != _nextState)
				_lastState = _nextState;

			_nextState = new State();

			#region Writing into _nextState from s_interfaceGameState
			{
				GSC.health temp = TickHandler.s_interfaceGameState.GetHealth(_id);
				if(null != temp)
				{
					_nextState._health = temp._health;
				}
			}
			#endregion
		}

		private void VisualizeLerpedStates()
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

			_health = Mathf.Lerp(_lastState._health, _nextState._health, lerpFactor);
		}

		#endregion

		#region IDestroyableBuilding
		public void TakeDamage(int amount)
		{
			_health -= amount;
			//set "i got hurt" flag to send to the client

			if(_health <= 0)
			{
				Die();
			}
		}

		private void Die() => transform.parent.gameObject.SetActive(false);
		public Transform GetTransform() => transform;
		public float GetHealth() => _health;
		public float GetMaxHealth() => _maxHealth;
		public int GetTeam() => _team;
		#endregion

		#region IPanelInfo
		private TextMeshProUGUI[] _panelDetails = new TextMeshProUGUI[0];
		public void InitialiseUnitPanel()
		{
			string[] details = new string[] { "Team: " + _team, "Health: " + (int)_health };
			UnitScreenController.s_instance.AddUnitInfoPanel(transform, details, ref _panelDetails);

			if(null != _myBoombox)
				_myBoombox.PlayClickSound();
		}

		public void UpdateUnitPanelInfo()
		{
			if(_panelDetails != null && 2 <= _panelDetails.Length)
			{
				_panelDetails[0].text = "Team: " + _team;
				_panelDetails[1].text = "Health: " + (int)_health;
			}
		}
		#endregion
	}
}