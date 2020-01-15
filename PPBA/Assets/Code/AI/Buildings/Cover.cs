using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace PPBA
{
	public class Cover : MonoBehaviour, INetElement, IPanelInfo, IDestroyableBuilding
	{
		protected class State
		{
			public Vector3 position;
			public float _angle;
			public float _health;
		}

		#region Variables
		//public
		public int _id { get; set; }
		public Arguments _arguments = new Arguments();
		[SerializeField] public int _team = 0;
		[SerializeField] public float _health { get => _healthBackingField; set => _healthBackingField = Mathf.Clamp(value, 0, _maxHealth); }
		[SerializeField] public float _maxHealth = 100;

		//private
		private float _healthBackingField = 100;
		#endregion

		#region References
		//public
		public CoverSlot[] _coverSlots;
		//private
		private State _lastState;
		private State _nextState;
		#endregion

		void Awake()
		{
#if !UNITY_SERVER
			TickHandler.s_DoInput += ExtractFromGameState;
#else
			TickHandler.s_GatherValues += WriteToGameState;
#endif
		}

		void Start()
		{

		}

		void Update()
		{

		}


		private void OnValidate()
		{
			_coverSlots = GetComponentsInChildren<CoverSlot>();

			for(int i = 0; i < _coverSlots.Length; i++)
			{
				_coverSlots[i]._parentCover = this;
			}
		}

		#region Interfaces
		#region INetElement
		public void ClearFlags(int tick)
		{
			_arguments = new Arguments();
		}

		public void ExtractFromGameState(int tick)//if CLIENT: an doinput hängen
		{
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

		public void WriteToGameState(int tick) => TickHandler.s_interfaceGameState.Add(new GSC.health { _id = _id, _health = _health, _morale = 0f });
		#endregion

		#region IPanelInfo
		private TextMeshProUGUI[] _panelDetails;
		public void InitialiseUnitPanel()
		{
			UnitScreenController.s_instance.AddUnitInfoPanel(transform, "Team: " + _team, "Health: " + _health, ref _panelDetails);
		}

		public void UpdateUnitPanelInfo()
		{
			if(_panelDetails != null && 3 <= _panelDetails.Length)
			{
				_panelDetails[0].text = "Team: " + _team;
				_panelDetails[1].text = "Health: " + _health;
			}
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
		public Transform GetTransform() => transform;
		public float GetHealth() => _health;
		public float GetMaxHealth() => _maxHealth;
		public int GetTeam() => _team;
		#endregion
		#endregion

		private static void ResetToDefault(Cover cover)
		{
			cover._health = cover._maxHealth;
		}

		private void AddCoverSlotsToLists()
		{
			for(int i = 0; i < _coverSlots.Length; i++)
			{
				JobCenter.s_coverSlots[_team].Add(_coverSlots[i]);
			}
		}

		private void RemoveCoverSlotsFromLists()
		{
			for(int i = 0; i < _coverSlots.Length; i++)
			{
				if(JobCenter.s_coverSlots[_team].Contains(_coverSlots[i]))
					JobCenter.s_coverSlots[_team].Remove(_coverSlots[i]);
			}
		}

		public static void Spawn(Vector3 spawnPoint, int team)
		{
			Cover newCover = (Cover)ObjectPool.s_objectPools[GlobalVariables.s_instance._prefabs[(int)ObjectType.COVER]].GetNextObject(team);
			ResetToDefault(newCover);
			newCover.transform.position = spawnPoint;

#if UNITY_SERVER
			newCover.AddCoverSlotsToLists();
#endif
		}

		private void Die()
		{
			Debug.Log("Cover " + _id + " of team " + _team + " got destroyed.");
			transform.parent.gameObject.SetActive(false);
		}

		private void OnEnable()
		{
			ResetToDefault(this);
		}

		private void OnDisable()
		{
#if UNITY_SERVER
			RemoveCoverSlotsFromLists();
#endif
			gameObject.SetActive(false);
		}
	}
}
