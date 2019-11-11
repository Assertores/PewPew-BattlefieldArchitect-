using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public enum MoraleEvents { IDLE, GETDAMAGED, CLOSEPAWNDIED, CLOSEPAWNDAMAGED, ENEMYDIED, ENEMYDAMAGED, WONGAME };

	public class Moralizer : Singleton<Moralizer>
	{
		[SerializeField] private float _idle = 0f;
		[SerializeField] private float _getDamaged = -10f;
		[SerializeField] private float _closePawnDied = -10f;
		[SerializeField] private float _closePawnDamaged = -5f;
		[SerializeField] private float _enemyDied = 10f;
		[SerializeField] private float _enemyDamaged = 5f;
		[SerializeField] private float _wonGame = 100f;

		void Start()
		{

		}

		void Update()
		{

		}

		/// <summary>
		/// Admins the intensity of morale change in case of events.
		/// This is put together here to make balancing less of a treasure hunt.
		/// </summary>
		/// <param name="moraleEvent"></param>
		/// <returns></returns>
		public float PassJudgement(MoraleEvents moraleEvent)
		{
			switch(moraleEvent)
			{
				case MoraleEvents.IDLE:
					return _idle;
				case MoraleEvents.GETDAMAGED:
					return _getDamaged;
				case MoraleEvents.CLOSEPAWNDIED:
					return _closePawnDied;
				case MoraleEvents.CLOSEPAWNDAMAGED:
					return _closePawnDamaged;
				case MoraleEvents.ENEMYDIED:
					return _enemyDied;
				case MoraleEvents.ENEMYDAMAGED:
					return _enemyDamaged;
				case MoraleEvents.WONGAME:
					return _wonGame;
				default:
					return 0f;
			}
		}
	}
}