using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class MediCamp : MonoBehaviour, INetElement, IDestroyableBuilding
	{
		[SerializeField] private MediCampHolder _myMediCampHolder;

		public int _id { get; set; }
		[SerializeField] public float _health = 1000;
		[SerializeField] private float _maxHealth = 1000;

		private class State
		{
			public Vector3 _position;
			public float _angle;
		}

		private State _lastState = new State();
		private State _nextState = new State();

		[SerializeField] public static float _interactRadius = 1.5f;

		private int _team => _myMediCampHolder._team;

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
			//TickHandler.s_interfaceGameState._transforms.Add(new GSC.transform { _id = _id, _position = transform.position, _angle = transform.eulerAngles.y });
		}

		private void ExtractFromGameState(int tick = 0)
		{

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
	}
}