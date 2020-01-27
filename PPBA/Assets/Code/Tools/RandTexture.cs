using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	[System.Serializable]
	struct RanTexElement
	{
		public Texture _albedo;
		public Texture _hight;
	}
	public class RandTexture : MonoBehaviour
	{
		[SerializeField]RanTex _ranTex;
		void Start()
		{
			MeshRenderer[] mr = GetComponentsInChildren<MeshRenderer>();

			int ran = Random.Range(0, _ranTex._ranTexs.Length);

			foreach(var it in mr)
			{
				it.material.SetTexture("_MainTex", _ranTex._ranTexs[ran]._albedo);
				it.material.SetTexture("_ParallaxMap", _ranTex._ranTexs[ran]._hight);
			}

			Destroy(this);
		}
	}

	[CreateAssetMenu(fileName = "RanTexAsset"/*, menuName = "ScriptableObjects/SpawnManagerScriptableObject", order = 1*/)]
	class RanTex : ScriptableObject
	{
		public RanTexElement[] _ranTexs;
	}
}
