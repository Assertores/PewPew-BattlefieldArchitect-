using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class HeatMapCalcRoutine : MonoBehaviour
	{
		private bool isRunning = false;

		public HeatMapReturnValue GetValues
		{
			get => HeatMap;
		}
		private HeatMapReturnValue HeatMap;


		public bool StartCalculate()
		{
			if(isRunning)
			{
				StartCoroutine(CalculateRoutine());
				return true;
			}
			else
			{
				return false;
			}
		}



		IEnumerator CalculateRoutine()
		{
			isRunning = true;


			//Print the time of when the function is first called.
			Debug.Log("Started Coroutine at timestamp : " + Time.time);

			//yield on a new YieldInstruction that waits for 5 seconds.
			yield return new WaitForSeconds(5);

			//After we have waited 5 seconds print the time again.
			Debug.Log("Finished Coroutine at timestamp : " + Time.time);


			isRunning = false;
		}



	}
}
