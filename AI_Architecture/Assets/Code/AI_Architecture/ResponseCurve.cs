using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ResponseCurve
{
    public enum CurveType { Linear, Quadratic, Logistic, Logit };
    
    /// <summary>
    /// takes function calls:
    /// (x, CurveType, m, k, b, c)
    /// Clamping input (0..1)
    /// Calc using appropriate formula type
    /// Clamp output (0..1)
    /// return y
    /// </summary>
    public static float RespondCurve(float x, CurveType type, float m, float k, float b, float c)
    {
        x = Mathf.Clamp(x, 0f, 1f);

        switch (type)
        {
            case CurveType.Linear:
                return Mathf.Clamp(m * Mathf.Pow(x - c, 1) + b, 0f, 1f);//lets k = 1, as Linear and Quadratic otherwise share the same formula
            case CurveType.Quadratic:
                return Mathf.Clamp(m * Mathf.Pow(x - c, k) + b, 0f, 1f);
            case CurveType.Logistic:
                return Mathf.Clamp(k / (1+1000*Mathf.Exp(1f)*Mathf.Pow(m, (c-x))), 0f, 1f);
            case CurveType.Logit:
                //return Mathf.Clamp((Mathf.Log(x/(1-x))+5)/10, 0f, 1f);    //this isn't properly parameterized yet
                return 1;
            default:
                Debug.LogWarning("RespondCurve defaulted to 1. Probably happened because of a faulty CurveType.");
                return 1;//defaulting to 1 to have no effect on behavior-score
        }
    }
}
