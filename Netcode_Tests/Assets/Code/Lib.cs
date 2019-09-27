using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Lib {
	public static float mod(float a, float b) {
		return a - b * Mathf.Floor(a / b);
	}

	public static int mod(int a, int b) {
		return a - b * Mathf.FloorToInt((float)a / b);
	}
}
