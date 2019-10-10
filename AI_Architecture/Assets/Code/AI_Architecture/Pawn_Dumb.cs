using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn_Dumbass : Pawn
{
    // Start is called before the first frame update
    void Start()
    {
        ResponseCurve.RespondCurve(0.5f, ResponseCurve.CurveType.Quadratic, 1, 1, 1, 1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override float Evaluate()
    {
        throw new System.NotImplementedException();
    }

    public override void Execute(Behavior behavior)
    {
        throw new System.NotImplementedException();
    }
}
