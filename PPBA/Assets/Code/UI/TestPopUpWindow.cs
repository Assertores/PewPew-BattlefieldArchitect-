using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class TestPopUpWindow : MonoBehaviour
	{
#if UNITY_EDITOR
		UIPopUpWindowRefHolder window;

		void Start()
		{
			Debug.Log("opening test window");
			window = UIPopUpWindowHandler.s_instance.CreateWindow("Test");
			Debug.Log("test window is " + window.name);

			StartCoroutine(IECloseWindow());
		}

		IEnumerator IECloseWindow()
		{
			yield return new WaitForSeconds(5);

			Debug.Log("Closing test window");
			window.CloseWindow();
		}
#endif
	}
}