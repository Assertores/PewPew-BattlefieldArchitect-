using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	[RequireComponent(typeof(ResourceDepot))]
	public class PawnSpawner : MonoBehaviour
	{
		private ResourceDepot _resourceDepot;
		[SerializeField] private int _pawnCost = 50;
		private ObjectType[] _pawnTypes = new ObjectType[] { ObjectType.PAWN_HEALER, ObjectType.PAWN_PIONEER, ObjectType.PAWN_WARRIOR };

		void Awake()
		{
			_resourceDepot = GetComponent<ResourceDepot>();
		}

		void Start()
		{

		}

		void Update()
		{

		}

		public void DoTheThing(int tick = 0)
		{
			if(tick % 25 != 0 || !HasEnoughResources())//early skip
				return;

			int[] schedule = GlobalVariables.GetScheduledPawns(_resourceDepot._team);
			int ticker = Random.Range(0, 3);

			for(int i = 0; i < 3; i++)
			{
				if(0 < schedule[ticker])
				{
					Pawn.Spawn(_pawnTypes[ticker], transform.position, _resourceDepot._team);
					_resourceDepot.TakeResources(_pawnCost);
					schedule[ticker]--;
					break;
				}

				ticker = ++ticker % _pawnTypes.Length;
			}
		}

		private bool HasEnoughResources() => _resourceDepot._resources < _pawnCost;

		private void OnEnable()
		{
			TickHandler.s_DoTick += DoTheThing;
		}

		private void OnDisable()
		{
			TickHandler.s_DoTick -= DoTheThing;
		}
	}
}