using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class Cover : MonoBehaviour, INetElement, IPanelInfo
	{
		#region Variables
		//public
		public int _id { get; set; }
		[SerializeField] public int _team = 0;
		[SerializeField] public float _health { get => _healthBackingField; set => _healthBackingField = Mathf.Clamp(value, 0, _maxHealth); }
		[SerializeField] public float _maxHealth = 100;

		//private
		private float _healthBackingField = 100;
		#endregion

		#region References
		//public
		public CoverSlot[] _coverSlots;
		#endregion

		void Start()
		{

		}

		void Update()
		{

		}

		public void TakeDamage(int amount)
		{
			_health -= amount;
			//set "i got hurt" flag to send to the client

			if(_health <= 0)
			{
				//die
			}
		}

		private void OnValidate()
		{
			_coverSlots = GetComponentsInChildren<CoverSlot>();

			for(int i = 0; i < _coverSlots.Length; i++)
			{
				_coverSlots[i]._parentCover = this;
			}
		}

		public void WriteToGameState(int tick)
		{
			//add team to gamestate
			TickHandler.s_interfaceGameState._healths.Add(new GSC.health { _id = _id, _health = _health, _morale = 0f });
		}

		public void InitialiseUnitPanel()
		{
			throw new System.NotImplementedException();
		}

		public void UpdateUnitPanelInfo()
		{
			throw new System.NotImplementedException();
		}


		/// <summary>
		/// Resets a pawn to factory setting.
		/// </summary>
		/// <param name="pawn">The pawn to be reset.</param>
		/// <param name="team"></param>
		private static void ResetToDefault(Cover cover, int team)
		{
			cover._team = team;
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
			Cover newCover = (Cover)ObjectPool.s_objectPools[GlobalVariables.s_instance._prefabs[(int)ObjectType.COVER]].GetNextObject();
			ResetToDefault(newCover, team);
			newCover.AddCoverSlotsToLists();
			newCover.transform.position = spawnPoint;
		}

		private void OnDisable()
		{
			RemoveCoverSlotsFromLists();
		}
	}
}
