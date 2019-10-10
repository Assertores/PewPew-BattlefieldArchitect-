using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Behavior : Singleton<Behavior>
{
    [System.Serializable]
    public class Axis
    {
        public string name;
        public ResponseCurve.CurveType type;
        public float m, k, b, c;
    }

    [SerializeField] public Axis[] axes;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
        
    public virtual float Calculate(Pawn pawn)
    {
        float score = 1;
        for (int i = 0; i < axes.Length; i++)
        {
            score *= ResponseCurve.RespondCurve(AxisInputs(axes[i].name), axes[i].type, axes[i].m, axes[i].k, axes[i].b, axes[i].c);//determine x for axis and put x in responseCurve
        }
        return 0;
    }

    //public abstract float Calculate(Pawn pawn);
    public abstract float Execute(Pawn pawn);
    public abstract float AxisInputs(string name);//switch returning value/maxValue of the axis-variable //should be handled by delegate somehow
}
