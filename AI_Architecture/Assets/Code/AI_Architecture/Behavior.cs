using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Behavior : MonoBehaviour
{    
    [System.Serializable]
    public class Axis
    {
        public bool isEnabled;
        public string name;
        public AnimationCurve curve;
    }

    [SerializeField] public Axis[] pawn_axes;
    [SerializeField] public Axis[] target_axes;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public float Calculate(Pawn pawn)
    {
        float _score = 1;

        for (int i = 0; i < pawn_axes.Length; i++)//calculate pawn-score
        {
            if (pawn_axes[i].isEnabled)
                _score *= Mathf.Clamp(pawn_axes[i].curve.Evaluate(PawnAxisInputs(pawn, pawn_axes[i].name)), 0f, 1f);
        }

        if (_score == 0f)//early skip
            return 0f;

        return _score * FindBestTarget(pawn);
    }

    //abstract functions
    public abstract void Execute(Pawn pawn);
    protected abstract float PawnAxisInputs(Pawn pawn, string name);//switch returning value/maxValue of the axis-variable


    /// <summary>
    /// - Finds and saves bestTarget, so that it can be read with TargetAxisInputs() and used in Execute().
    /// - Uses CalculateTargetScore often.
    /// - Adds the <Pawn,Target> Tuple to a targetDictionary.
    /// </summary>
    public abstract float FindBestTarget(Pawn pawn);

    //also needs to implement:
    //protected float TargetAxisInputs(Pawn pawn, string name);//switch returning value/maxValue of the axis-variable
    //protected float CalculateTargetScore(Pawn pawn, TARGET target)
    //
    //as these cannot be defined here, as only the behavior itself knows the signature
}
