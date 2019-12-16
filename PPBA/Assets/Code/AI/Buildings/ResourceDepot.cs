using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class ResourceDepot : MonoBehaviour, INetElement
	{
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

		private IRefHolder _myRefHolder;

		void Awake()
		{
			_myRefHolder = GetComponentInParent<IRefHolder>();
		}

		void Start()
		{

		}

		void Update()
		{

		}

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
		private void OnEnable()
		{
#if UNITY_SERVER
			if(!JobCenter.s_resourceDepots[_team].Contains(this))
				JobCenter.s_resourceDepots[_team].Add(this);

			TickHandler.s_LateCalc += CalculateScore;
			TickHandler.s_GatherValues += WriteToGameState;
#endif
		}

		private void OnDisable()
		{
#if UNITY_SERVER
			if(JobCenter.s_resourceDepots[_team].Contains(this))
				JobCenter.s_resourceDepots[_team].Remove(this);

			TickHandler.s_LateCalc -= CalculateScore;
			TickHandler.s_GatherValues -= WriteToGameState;
#endif
			gameObject.SetActive(false);
		}

		public void WriteToGameState(int tick)
		{
			TickHandler.s_interfaceGameState._transforms.Add(new GSC.transform { _id = _id, _position = transform.position, _angle = transform.eulerAngles.y });
			TickHandler.s_interfaceGameState._ammos.Add(new GSC.ammo { _id = _id, _bullets = _ammo });
			TickHandler.s_interfaceGameState._resources.Add(new GSC.resource { _id = _id, _resources = _resources });
			TickHandler.s_interfaceGameState._healths.Add(new GSC.health { _id = _id, _health = _health, _morale = _score });
		}
		#endregion
	}
}