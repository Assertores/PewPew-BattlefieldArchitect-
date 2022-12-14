using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	[RequireComponent(typeof(AudioSource))]
	public class UIBoomboxController : MonoBehaviour
	{
		private static AudioSource _source;

		void Awake()
		{
			_source = GetComponent<AudioSource>();
		}

		void Start()
		{
			if(null == _source)
				return;

			_source.clip = AudioWarehouse._defaultClip;
		}

		void Update()
		{

		}

		public static void PlayUI(ClipsUI _clip) => _source?.PlayOneShot(AudioWarehouse.s_instance.Clip(_clip));
	}
}