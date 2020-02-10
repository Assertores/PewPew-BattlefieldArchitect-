using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	[RequireComponent(typeof(ResourceDepot))]
	public class PawnSpawner : MonoBehaviour
	{
		private ResourceDepot _resourceDepot;
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
			if(tick % 25 != 0 || _resourceDepot._resources < 50)//early skip
				return;

			int[] schedule = GlobalVariables.GetScheduledPawns(_resourceDepot._team);
			int ticker = Random.Range(0, 3);

			if(null == schedule)
				return;

			for(int i = 0; i < 3; i++)
			{
				if(0 < schedule[ticker] && HasEnoughResources(_pawnTypes[ticker]))
				{
					Pawn.Spawn(_pawnTypes[ticker], transform.position, _resourceDepot._team);
					_resourceDepot.TakeResources(PawnCost(_pawnTypes[ticker]));
					schedule[ticker]--;
					break;
				}

				ticker = ++ticker % _pawnTypes.Length;
			}
		}

		private bool HasEnoughResources(ObjectType pawnType) => _resourceDepot._resources < PawnCost(pawnType);

		private int PawnCost(ObjectType pawnType)
		{
			switch(pawnType)
			{
				case ObjectType.PAWN_HEALER:
					return 50;
				case ObjectType.PAWN_PIONEER:
					return 75;
				case ObjectType.PAWN_WARRIOR:
					return 100;
				default:
					return 75;
			}
		}

		private void OnEnable()
		{
#if UNITY_SERVER
			TickHandler.s_DoTick += DoTheThing;
#endif
		}

		private void OnDisable()
		{
#if UNITY_SERVER
			TickHandler.s_DoTick -= DoTheThing;
#endif
		}
	}
}