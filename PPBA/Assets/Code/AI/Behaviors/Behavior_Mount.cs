using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class Behavior_Mount : Behavior
	{
		public static Behavior_Mount s_instance;
		public static Dictionary<Pawn, MountSlot> s_targetDictionary;

		[SerializeField] [Tooltip("How many resources does a pawn grab at once?")] private int _grabSize = 10;

		private void Awake()
		{
			if(s_instance == null)
				s_instance = this;
			else
				Destroy(gameObject);
		}

		public override void Execute(Pawn pawn)
		{
			throw new System.NotImplementedException();
		}

		public override float FindBestTarget(Pawn pawn)
		{
			throw new System.NotImplementedException();
		}

		public override int GetTargetID(Pawn pawn)
		{
			throw new System.NotImplementedException();
		}

		protected override float PawnAxisInputs(Pawn pawn, string name)
		{
			throw new System.NotImplementedException();
		}

		void Start()
		{
			
		}

		void Update()
		{
			
		}
	}
}