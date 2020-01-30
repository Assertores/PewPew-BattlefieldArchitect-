using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace PPBA
{
	public class ResourceDepot : MonoBehaviour, INetElement, IDestroyableBuilding, IPanelInfo
	{
		private class State
		{
			public float _health;
			public float _ammo;
			public int _supplies;
		}

		private State _lastState = new State();
		private State _nextState = new State();

		[SerializeField] public int _id { get; set; }
		public int _team
		{
			get
			{
				if(null == _myRefHolder)
					_myRefHolder = GetComponentInParent<IRefHolder>();

				if(null == _myRefHolder)
					return 0;
				else
					return _myRefHolder._team;
			}
		}
		[SerializeField] public float _health = 1000;
		[SerializeField] public float _maxHealth = 1000;
		[SerializeField] public int _resources = 0;
		[SerializeField] public int _maxResources = 1000;
		[SerializeField] public int _ammo = 0;
		[SerializeField] public int _maxAmmo = 1000;
		[SerializeField] public float _score = 0;
		[SerializeField] public float _maxScore = 1;
		[SerializeField]
		[Tooltip("How close does a pawn have to be to interact with this?")]
		public float _interactRadius = 2f;
		/// <summary>
		/// Use _resourceTotal[team] to access resource total over all depots of that team.
		/// Ready to use after DoInput. 
		/// </summary>
		public static int[] _resourceTotal = new int[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, };

		private IRefHolder _myRefHolder;

		#region Monobehaviour
		void Awake()
		{
			_myRefHolder = GetComponentInParent<IRefHolder>();
		}
		void Update()
		{
#if !UNITY_SERVER
			VisualizeLerpedStates();
#endif
		}
		#endregion

		public void CalculateScore(int tick = 0)
		{
			_score = 0;

			if(0 == _resources)
				return;

			//determine proximity and weight of build jobs
			foreach(Blueprint b in JobCenter.s_blueprints[_team])
			{
				//score has to be normalised somehow. what would be a good max?
				_score += b._resourcesNeeded / Vector3.Magnitude(b.transform.position - transform.position);
			}

			_score = Mathf.Clamp(_score, 0f, 1f);
		}

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

		#region Give Or Take
		public int TakeResources(int amount)
		{
			if(amount <= _resources)//can take full wanted amount
			{
				_resources -= amount;
				return amount;
			}
			else//can only take a part of the wanted amount
			{
				int temp = _resources;
				_resources = 0;
				return temp;
			}
		}

		public int GiveResources(int amount)
		{
			int spaceLeft = _maxResources - _resources;

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

		public int TakeAmmo(int amount)
		{
			if(amount <= _ammo)//can take full wanted amount
			{
				_ammo -= amount;
				return amount;
			}
			else//can only take a part of the wanted amount
			{
				int temp = _ammo;
				_ammo = 0;
				return temp;
			}
		}

		public int GiveAmmo(int amount)
		{
			int spaceLeft = _maxAmmo - _ammo;

			if(amount <= spaceLeft)//enough space
			{
				_ammo += amount;
				return amount;
			}
			else//not enough space
			{
				_ammo += spaceLeft;
				return spaceLeft;
			}
		}
		#endregion

		#region Initialisation
		private void ResetToDefault()
		{
			_health = _maxHealth;
			_ammo = 0;
			_resources = 0;
		}

		private void OnEnable()
		{
#if UNITY_SERVER
			ResetToDefault();

			if(null != JobCenter.s_resourceDepots?[_team] && !JobCenter.s_resourceDepots[_team].Contains(this))
				JobCenter.s_resourceDepots[_team].Add(this);

			TickHandler.s_LateCalc += CalculateScore;
			TickHandler.s_GatherValues += WriteToGameState;
#else
			TickHandler.s_SetUp += ClearTotal;
			TickHandler.s_DoInput += ExtractFromGameState;
			TickHandler.s_EarlyCalc += ActivateBuildingMenu;
#endif
		}

		private void OnDisable()
		{
#if UNITY_SERVER
			if(null != JobCenter.s_resourceDepots?[_team] && JobCenter.s_resourceDepots[_team].Contains(this))
				JobCenter.s_resourceDepots[_team].Remove(this);

			TickHandler.s_LateCalc -= CalculateScore;
			TickHandler.s_GatherValues -= WriteToGameState;
#else
			TickHandler.s_SetUp -= ClearTotal;
			TickHandler.s_DoInput -= ExtractFromGameState;
			TickHandler.s_EarlyCalc -= ActivateBuildingMenu;
#endif
			gameObject.SetActive(false);
		}
		#endregion

		public void WriteToGameState(int tick = 0)
		{
			//TickHandler.s_interfaceGameState.Add(new GSC.transform { _id = _id, _position = transform.position, _angle = transform.eulerAngles.y });
			TickHandler.s_interfaceGameState.Add(new GSC.ammo { _id = _id, _bullets = _ammo });
			TickHandler.s_interfaceGameState.Add(new GSC.resource { _id = _id, _resources = _resources });
			TickHandler.s_interfaceGameState.Add(new GSC.health { _id = _id, _health = _health, _morale = _score });
			TickHandler.s_interfaceGameState.Add(new GSC.type { _id = _id, _type = 0, _team = (byte)_team });
		}

		public void ExtractFromGameState(int tick = 0)
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
			{
				GSC.ammo temp = TickHandler.s_interfaceGameState.GetAmmo(_id);

				if(null != temp)
				{
					_nextState._ammo = temp._bullets;
				}
			}
			{
				GSC.resource temp = TickHandler.s_interfaceGameState.GetResource(_id);
				GSC.type typeTemp = TickHandler.s_interfaceGameState.GetType(_id);

				if(null != temp)
				{
					_nextState._supplies = temp._resources;

					if(null != typeTemp)
						_resourceTotal[typeTemp._team] += temp._resources;
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
			_ammo = (int)Mathf.Lerp(_lastState._ammo, _nextState._ammo, lerpFactor);
			_resources = (int)Mathf.Lerp(_lastState._supplies, _nextState._supplies, lerpFactor);
		}

		public void ClearTotal(int tick = 0) => _resourceTotal = new int[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, };

		void ActivateBuildingMenu(int tick)
		{
			TickHandler.s_EarlyCalc -= ActivateBuildingMenu;

			if(TickHandler.s_interfaceGameState.GetType(_id)?._team != GlobalVariables.s_instance._clients[0]._id)
				return;

			print(tick + "\n" + TickHandler.s_interfaceGameState.ToString());
			print("_team and global ID " + _team + ", " + GlobalVariables.s_instance._clients[0]._id);
			print((GetComponentInParent<IRefHolder>() as MonoBehaviour).name);

			UiInventory.s_instance.AddLastBuildings();
		}

		#region IPanelInfo
		private TextMeshProUGUI[] _panelDetails = new TextMeshProUGUI[0];
		public void InitialiseUnitPanel()
		{
			string[] details = new string[] { "Team: " + _team, "Health: " + (int)_health, "Supplies: " + _resources, "Ammo: " + _ammo };
			UnitScreenController.s_instance.AddUnitInfoPanel(transform, details, ref _panelDetails);
		}

		public void UpdateUnitPanelInfo()
		{
			if(_panelDetails != null && 4 <= _panelDetails.Length)
			{
				_panelDetails[0].text = "Team: " + _team;
				_panelDetails[1].text = "Health: " + (int)_health;
				_panelDetails[2].text = "Supplies: " + _resources;
				_panelDetails[3].text = "Ammo: " + _ammo;
			}
		}
		#endregion
	}
}