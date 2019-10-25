using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public static class Lib
	{
		public static float Mod(float a, float b)
		{
			return a - b * Mathf.Floor(a / b);
		}

		public static int Mod(int a, int b)
		{
			return a - b * Mathf.FloorToInt((float)a / b);
		}
	}
}