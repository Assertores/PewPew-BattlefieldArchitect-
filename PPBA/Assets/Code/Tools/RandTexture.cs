using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{

	public class RandTexture : MonoBehaviour
	{
		[SerializeField] RandModelAsset _ranTex;
		void Start()
		{
			MeshRenderer mr = GetComponent<MeshRenderer>();
			MeshFilter mf = GetComponent<MeshFilter>();

			int ran = Random.Range(0, _ranTex._ranTexs.Length);

			mf.mesh = _ranTex._meshs[Random.Range(0, _ranTex._meshs.Length)];

			mr.material.SetTexture("_MainTex", _ranTex._ranTexs[ran]._albedo);
			mr.material.SetTexture("_ParallaxMap", _ranTex._ranTexs[ran]._hight);

			Destroy(this);
		}
	}
}
