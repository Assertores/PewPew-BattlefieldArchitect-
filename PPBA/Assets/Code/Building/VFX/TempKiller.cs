using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class TempKiller : MonoBehaviour
	{
		void Start()
		{
			
		}

		void Update()
		{
			
		}

		private void OnEnable()
		{
			Destroy(gameObject, 7);
		}
	}
}