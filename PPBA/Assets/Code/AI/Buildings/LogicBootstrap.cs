using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class LogicBootstrap : MonoBehaviour
	{
		void Start()
		{
			
		}

		void Update()
		{
			
		}

		private void OnDisable()
		{
#if !UNITY_SERVER
			gameObject.SetActive(false);
#endif
		}
	}
}