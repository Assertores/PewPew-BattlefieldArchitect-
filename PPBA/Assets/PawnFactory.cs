using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class PawnFactory : MonoBehaviour
	{
		[Tooltip("Maximum number of pawns spawned.")] public int _maxPawns = 10;
		private int _ticker = 0;

		void Start()
		{

		}

		void Update()
		{

		}

		private void FixedUpdate()
		{
			if(_ticker < _maxPawns)
			{
				SpawnPawn();
				_ticker++;
				//Debug.Log(_ticker);
			}
		}

		private void SpawnPawn()
		{
			ObjectType pawnType = Pawn.RandomPawnType();
			int team = Random.Range(0, 2);
			Pawn.Spawn(pawnType, transform.position + Vector3.forward * _ticker, team);
		}
	}
}