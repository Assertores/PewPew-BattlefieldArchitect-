using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class AntiGhostFix : MonoBehaviour
	{
		[SerializeField] private Blueprint _myBlueprint;

		void Start()
		{
			
		}

		void Update()
		{
			_myBlueprint.SetClipFull();
		}

		private void OnValidate()
		{
			if(null == _myBlueprint)
				_myBlueprint = transform.parent?.GetChild(1)?.GetComponent<Blueprint>();
		}
	}
}