using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	[RequireComponent(typeof(AudioSource))]
	public class MusicBoomboxController : MonoBehaviour
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

			PlayMusic(ClipsMusic.MUSIC_TRACK_01);
		}

		void Update()
		{

		}

		public void PlayMusic(ClipsMusic _clip)
		{
			AudioClip temp = AudioWarehouse.s_instance.Clip(_clip);

			if(_source.clip == temp)
			{
				if(!_source.isPlaying)
					_source.Play();
				return;
			}
			else
			{
				_source.clip = temp;
				_source.Play();
			}
		}
	}
}