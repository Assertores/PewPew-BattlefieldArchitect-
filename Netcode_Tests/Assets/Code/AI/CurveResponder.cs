using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CurveResponder
{
    public enum CurveType { Linear, Quadratic, Logistic, Logit };

    //takes function calls:
    //(x, CurveType, m, k, b, c)
    //Clamping input (0..1)
    //Calc using appropriate formula type
    //Clamp output (0..1)
    //return y

    public static float RespondCurve(float x, CurveType type, float m, float k, float b, float c)
    {
        switch (type)
        {
            case CurveType.Linear:
                return 1;
            case CurveType.Quadratic:
                return 1;
            case CurveType.Logistic:
                return 1;
            case CurveType.Logit:
                return 1;
            default:
                return 0;
        }
    }
}
