using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace PPBA
{
	public class RefineryRefHolder : MonoBehaviour
	{
		public BuildType Type = BuildType.REFINERY;
		public GameObject _BuildPrefab;
		public AudioClip _BuildingSound;
		public Image _ImageUI;


		public float _harvestRadius;
		public float _harvestIntensity;

	}

}


