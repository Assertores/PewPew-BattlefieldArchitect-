using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public abstract class Behavior : MonoBehaviour
	{
		[System.Serializable]
		public class Axis
		{
			public bool _isEnabled;
			public string _name;
			public AnimationCurve _curve;
		}

		[SerializeField] public Axis[] _pawnAxes;
		[SerializeField] public Axis[] _targetAxes;

		void Start()
		{

		}

		void Update()
		{

		}

		public float Calculate(Pawn pawn)
		{
			float score = 1f;

			for(int i = 0; i < _pawnAxes.Length; i++)//calculate pawn-score
			{
				if(_pawnAxes[i]._isEnabled)
					score *= Mathf.Clamp(_pawnAxes[i]._curve.Evaluate(PawnAxisInputs(pawn, _pawnAxes[i]._name)), 0f, 1f);
			}

			if(score == 0f)//early skip
				return 0f;

			return score * FindBestTarget(pawn);
		}

		//abstract functions
		public abstract void Execute(Pawn pawn);
		protected abstract float PawnAxisInputs(Pawn pawn, string name);//switch returning value/maxValue of the axis-variable
		
		/// <summary>
		/// - Finds and saves bestTarget, so that it can be read with TargetAxisInputs() and used in Execute().//but i'm using targetAxisInputs() for that calc? óÒ
		/// - Uses CalculateTargetScore often.
		/// - Adds the <Pawn,Target> Tuple to a targetDictionary.
		/// </summary>
		public abstract float FindBestTarget(Pawn pawn);

		//also needs to implement:
		//protected float TargetAxisInputs(Pawn pawn, string name, TARGET target);//switch returning value/maxValue of the axis-variable
		//protected float CalculateTargetScore(Pawn pawn, TARGET target)
		//
		//as these cannot be defined here, as only the behavior itself knows the signature
		//
		//got to remove pawns from the TARGETDICTIONARIES when they change behavior

		public abstract int GetTargetID(Pawn pawn);
		public abstract void RemoveFromTargetDict(Pawn pawn);
	}
}