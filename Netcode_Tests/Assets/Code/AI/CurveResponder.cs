﻿using System.Collections;
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
                return Mathf.Clamp(m * Mathf.Pow(x-c, k) + b, 0f, 1f);
            case CurveType.Quadratic:
                return Mathf.Clamp(m * Mathf.Pow(x - c, k) + b, 0f, 1f);
            case CurveType.Logistic:
                return 1; //Mathf.Clamp(k / (1+Mathf.));
            case CurveType.Logit:
                return 1;
            default:
                return 0;
        }
    }
}
