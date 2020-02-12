using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class ExplosionSpawner : MonoBehaviour
	{
		public enum Bombs { BigDaddy, MiddleMan, LittleTwink };

		[SerializeField] private GameObject _bigDaddy;
		[SerializeField] private GameObject _middleMan;
		[SerializeField] private GameObject _littleTwink;

		[SerializeField] private Bombs _whichBomb;

		void Start()
		{
			
		}

		void Update()
		{
			
		}

		private void OnDisable()
		{
#if !UNITY_SERVER
			GameObject newBomb = GameObject.Instantiate(PickYourPoison(_whichBomb));
			newBomb.SetActive(true);
#endif
		}

		private GameObject PickYourPoison(Bombs myMan)
		{
			switch(myMan)
			{
				case Bombs.BigDaddy:
					return _bigDaddy;
				case Bombs.MiddleMan:
					return _middleMan;
				case Bombs.LittleTwink:
					return _littleTwink;
				default:
					return _bigDaddy;
			}
		}
	}
}