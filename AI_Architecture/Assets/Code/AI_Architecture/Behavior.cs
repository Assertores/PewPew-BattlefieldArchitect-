using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Behavior : MonoBehaviour
{
    //public
    [System.Serializable]
    public class Axis
    {
        public bool isEnabled;
        public string name;
        public ResponseCurve.CurveType type;
        public float m, k, b, c;
    }

    [SerializeField] public Axis[] pawn_axes;
    [SerializeField] public Axis[] target_axes;

    //protected
    protected float targetScore;

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
        float score = 1;

        for (int i = 0; i < pawn_axes.Length; i++)//calculate pawn-score
        {
            if (pawn_axes[i].isEnabled)
                score *= ResponseCurve.RespondCurve(PawnAxisInputs(pawn, pawn_axes[i].name), pawn_axes[i].type, pawn_axes[i].m, pawn_axes[i].k, pawn_axes[i].b, pawn_axes[i].c);//determine x for axis and put x in responseCurve
        }

        if (score == 0f)//early skip
            return 0f;

        FindBestTarget(pawn);
        score *= targetScore;

        return score;
    }

    protected float CalculateTargetScore(Pawn pawn)
    {
        float score = 1;

        for (int i = 0; i < target_axes.Length; i++)
        {
            if (target_axes[i].isEnabled)
                score *= ResponseCurve.RespondCurve(TargetAxisInputs(pawn, target_axes[i].name), target_axes[i].type, target_axes[i].m, target_axes[i].k, target_axes[i].b, target_axes[i].c);//determine x for axis and put x in responseCurve
        }

        return score;
    }

    //abstract functions
    public abstract void Execute(Pawn pawn);
    protected abstract float PawnAxisInputs(Pawn pawn, string name);//switch returning value/maxValue of the axis-variable
    protected abstract float TargetAxisInputs(Pawn pawn, string name);//switch returning value/maxValue of the axis-variable
    
    /// <summary>
    /// - Finds and saves bestTarget, so that it can be read with TargetAxisInputs() and used in Execute().
    /// - Saves targetScore of the best target.
    /// - Uses CalculateTargetScore often.
    /// </summary>
    public abstract void FindBestTarget(Pawn pawn);

    //public delegate float GetInputFunction();
    //public float LinearAxis() => 1;
    //public float QuadraticAxis() => 1;
    ////GetInputFunction getInput = new GetInputFunction()    
}
