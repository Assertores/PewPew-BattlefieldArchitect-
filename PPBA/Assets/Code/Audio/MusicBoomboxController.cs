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
			//if(null == _sources)
			//return;
		}

		void Update()
		{
			if(!_source.isPlaying)
			{
				_source.clip = RandomEnvironmentClip();
				_source.Play();
			}
		}

		private AudioClip RandomEnvironmentClip() => AudioWarehouse.s_instance.Clip(RandomEnvironmentEnum());

		private ClipsEnvironment RandomEnvironmentEnum()
		{
			switch(Random.Range(0, 4))
			{
				case 0:
					return ClipsEnvironment.ENVIRONMENT_BIRDS_01;
				case 1:
					return ClipsEnvironment.ENVIRONMENT_LIGHT_SNOWSTORM_01;
				case 2:
					return ClipsEnvironment.ENVIRONMENT_WAR_01;
				//case 3:
				//return ClipsEnvironment.ENVIRONMENT_WAR_01_PRE_LOOP;
				default:
					return ClipsEnvironment.ENVIRONMENT_WAR_01;
			}
		}

		/*
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
*/
	}
}