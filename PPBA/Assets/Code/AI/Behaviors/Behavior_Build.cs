using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class Behavior_Build : Behavior
	{
		public static Behavior_Build s_instance;
		public static Dictionary<Pawn, Blueprint> s_targetDictionary = new Dictionary<Pawn, Blueprint>();

		[SerializeField] [Tooltip("How many resources does a pawn grab at once?")] private int _grabSize = 10;

		public Behavior_Build()
		{
			_name = Behaviors.BUILD;
		}

		private void Awake()
		{
			if(s_instance == null)
				s_instance = this;
			else
				Destroy(gameObject);
		}

		void Start()
		{

		}

		void Update()
		{

		}

		public override void Execute(Pawn pawn)
		{
			if(s_targetDictionary.ContainsKey(pawn) && null != s_targetDictionary[pawn] && s_targetDictionary[pawn].isActiveAndEnabled)
			{
				Vector3 targetPosition = s_targetDictionary[pawn].transform.position;
				pawn.SetMoveTarget(targetPosition);

				if(Vector3.Magnitude(targetPosition - pawn.transform.position) < 10f)
				{
					//Takes an amount of resources from the depot no larger than (1) the space left at pawn (2) the res left at depot, and gives it to the pawn.
					s_targetDictionary[pawn].DoWork();
					//TakeResources(Mathf.Min(_grabSize, pawn._maxResource - pawn._resources));
				}
			}
		}

		public override float FindBestTarget(Pawn pawn)
		{
			float bestScore = 0f;

			foreach(Blueprint blueprint in JobCenter.s_blueprints[pawn._team])
			{
				float tempScore = CalculateTargetScore(pawn, blueprint);

				if(bestScore < tempScore)
				{
					s_targetDictionary[pawn] = blueprint;
					bestScore = tempScore;
				}
			}

			return bestScore;
		}

		protected override float PawnAxisInputs(Pawn pawn, string name)
		{
			switch(name)
			{
				case "Health":
					return pawn._health / pawn._maxHealth;
				default:
					Debug.LogWarning("PawnAxisInputs defaulted to 1. Probably messed up the string name: " + name);
					return 1;
			}
		}

		protected float TargetAxisInputs(Pawn pawn, string name, Blueprint blueprint)
		{
			switch(name)
			{
				case "Distance":
					return Vector3.Distance(pawn.transform.position, blueprint.transform.position) / 60f;
				/*
			case "Score":
				return blueprint._score;
			case "Resources":
				return blueprint._resources / blueprint._maxResources;
				*/
				default:
					Debug.LogWarning("TargetAxisInputs defaulted to 1. Probably messed up the string name: " + name);
					return 1;
			}
		}

		protected float CalculateTargetScore(Pawn pawn, Blueprint blueprint)
		{
			float _score = 1;

			for(int i = 0; i < _targetAxes.Length; i++)
			{
				if(_targetAxes[i]._isEnabled)
					_score *= Mathf.Clamp(_targetAxes[i]._curve.Evaluate(TargetAxisInputs(pawn, _targetAxes[i]._name, blueprint)), 0f, 1f);
			}

			return _score;
		}

		public override int GetTargetID(Pawn pawn)
		{
			if(s_targetDictionary.ContainsKey(pawn))
				return s_targetDictionary[pawn]._id;
			else
				return -1;
		}

		public override void RemoveFromTargetDict(Pawn pawn)
		{
			if(s_targetDictionary.ContainsKey(pawn))
				s_targetDictionary.Remove(pawn);
		}
	}
}