﻿using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using System.Text;

namespace PPBA
{
	//alle combined typen: cornername und between name müssen direkt hintereinander stehen in dieser reihenfolge
	//e.g. ..., WALL, WALL_BETWEEN, ...
	public enum ObjectType { REFINERY, DEPOT, GUN_TURRET, WALL, WALL_BETWEEN, PAWN_WARRIOR, PAWN_HEALER, PAWN_PIONEER, COVER, FLAGPOLE,HQ, MEDICAMP, SIZE };

	[System.Serializable]
	public class client
	{
		public int _id;
		public bool _isConnected = false;
		public IPEndPoint _ep;
		public RingBuffer_InputState _inputStates = new RingBuffer_InputState(); //public RingBuffer<InputState> _inputStates = new RingBuffer<InputState>();
		public RingBuffer_GameState _gameStates = new RingBuffer_GameState(); //public RingBuffer<GameState> _gameStates = new RingBuffer<GameState>();

		public client()
		{
			_inputStates[0] = new InputState();
			_gameStates[0] = new GameState();
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine(base.ToString());
			sb.AppendLine("id: " + _id.ToString());
			sb.AppendLine("is Connected: " + _isConnected.ToString());
			sb.AppendLine("endPoint: " + (_ep == null ? "NON" : _ep.ToString()));
			sb.AppendLine("Input State: " + (_inputStates == null ? "NON" : _inputStates.ToString()));
			sb.AppendLine("Game State: " + (_gameStates == null ? "NON" : _gameStates.ToString()));

			return sb.ToString();
		}
	}

	public class GlobalVariables : Singleton<GlobalVariables>
	{
		#region Variables

		public List<client> _clients = new List<client>();

		[Tooltip("deactivate this bool to signivy that this variables where set by us")]
		[SerializeField] private bool _autoGenerated = true;

		[System.Serializable]
		struct BuildAssignment
		{
			public ObjectType _type;
			public GameObject _prefab;
		}
		[SerializeField] private BuildAssignment[] _prefabInput;
		[SerializeField] [Range(1, 100)] private int _initialObjectPoolSize = 100;

		[SerializeField] public Color[] _teamColors;
		[HideInInspector] public GameObject[] _prefabs;


		#endregion
		#region MonoBehaviour

		void Start()
		{
			//place variables or calculations that have to be set/done here
#if !UNITY_SERVER
			{
				client tmp = new client();
				_clients.Insert(0, tmp);
			}
#endif

			if(_autoGenerated)
			{
				Debug.LogError("Global Values where automaticly generated");
				return;
			}

			//place variables or calculation that must not be set or done if it is autogenerated

			_prefabs = new GameObject[(int)ObjectType.SIZE];
			foreach(var it in _prefabInput)
			{
				_prefabs[(int)it._type] = it._prefab;
			}

			//ObjectPool.CreatePool<Pawn>(_prefabs[(int)ObjectType.PAWN_WARRIOR], 100, transform); //nicht übers netzwerk

			ObjectPool.CreatePool<RefineryRefHolder>(ObjectType.REFINERY, _initialObjectPoolSize, transform); //über netzwerk getracked
			ObjectPool.CreatePool<WallRefHolder>(ObjectType.WALL, _initialObjectPoolSize * 2, transform); //über netzwerk getracked
			ObjectPool.CreatePool<WallRefHolder>(ObjectType.WALL_BETWEEN, _initialObjectPoolSize * 2, transform); //über netzwerk getracked
			//ObjectPool.CreatePool<WallRefHolder>(ObjectType.WALL, 100, transform); //über netzwerk getracked
			ObjectPool.CreatePool<Pawn>(ObjectType.PAWN_WARRIOR, _initialObjectPoolSize, transform); //über netzwerk getracked
			ObjectPool.CreatePool<Pawn>(ObjectType.PAWN_HEALER, _initialObjectPoolSize, transform); //über netzwerk getracked
			ObjectPool.CreatePool<Pawn>(ObjectType.PAWN_PIONEER, _initialObjectPoolSize, transform); //über netzwerk getracked
			ObjectPool.CreatePool<HQHolder>(ObjectType.HQ, 10, transform); //über netzwerk getracked
			ObjectPool.CreatePool<TrashWallHolder>(ObjectType.COVER, _initialObjectPoolSize, transform); //über netzwerk getracked
			ObjectPool.CreatePool<MediCampHolder>(ObjectType.MEDICAMP, _initialObjectPoolSize, transform); //über netzwerk getracked
			ObjectPool.CreatePool<FlagHolder>(ObjectType.FLAGPOLE, _initialObjectPoolSize, transform); //über netzwerk getracked
						
			//ObjectPool.CreatePool<Cover>(ObjectType.COVER, _initialObjectPoolSize, transform); //über netzwerk getracked
			//Pawn nextPawn = (Pawn)ObjectPool.s_objectPools[_prefabs[(int)ObjectType.PAWN_WARRIOR]].GetNextObject();
		}

		#endregion
	}
}
