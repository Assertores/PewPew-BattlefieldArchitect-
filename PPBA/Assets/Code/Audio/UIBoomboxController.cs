using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	[RequireComponent(typeof(AudioSource))]
	public class UIBoomboxController : MonoBehaviour
	{
		private AudioSource _source;

		void Awake()
		{
			_source = GetComponent<AudioSource>();
		}

		void Start()
		{
			if(null == _source)
				return;

			_source.clip = AudioWarehouse.s_instance._defaultClip;
		}

		void Update()
		{

		}

		public void PlayUI(ClipsUI _clip) => _source.PlayOneShot(AudioWarehouse.s_instance.Clip(_clip));
	}
}