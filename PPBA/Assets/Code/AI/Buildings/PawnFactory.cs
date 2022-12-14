using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class PawnFactory : MonoBehaviour
	{
		[Tooltip("Maximum number of pawns spawned.")] public int _maxPawns = 10;
		private int _ticker = 0;
		[SerializeField] private bool _isOneTeam = false;

		void Start()
		{

		}

		void Update()
		{

		}

		private void FixedUpdate()
		{
#if UNITY_SERVER
			if(_ticker < _maxPawns)
			{
				//SpawnPawn();
				//_ticker++;
				//Debug.Log(_ticker);
			}
#endif
		}

		private void SpawnPawn(int tick = 0)
		{
			if(_ticker < _maxPawns)
			{
				//ObjectType pawnType = Pawn.RandomPawnType();
				ObjectType pawnType = ObjectType.PAWN_PIONEER;
				int team = !_isOneTeam && _ticker % 2 == 0 ? 1 : 0;
				Pawn.Spawn(pawnType, transform.position + Vector3.forward * _ticker * 2f, team);

				float temp = _ticker == 0 ? 1f : -1f;
				FlagPole.Spawn(transform.position + Vector3.right * temp * 10f, team);

				_ticker++;
			}
		}

		private void OnEnable()
		{
#if UNITY_SERVER
			TickHandler.s_DoTick += SpawnPawn;
#endif
		}
	}
}