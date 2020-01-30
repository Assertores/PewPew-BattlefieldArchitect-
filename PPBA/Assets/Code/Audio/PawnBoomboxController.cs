using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	[RequireComponent(typeof(AudioSource))]
	[RequireComponent(typeof(AudioSource))]
	[RequireComponent(typeof(AudioSource))]
	public class PawnBoomboxController : MonoBehaviour
	{
		private AudioSource[] _sources;

		private AudioSource _stepSource;
		private AudioSource _behaviorSource;
		private AudioSource _voiceSource;

		void Awake()
		{
			_sources = GetComponents<AudioSource>();
		}

		void Start()
		{
			if(null == _sources)
				return;

			for(int i = 0; i < _sources.Length; i++)
				_sources[i].clip = AudioWarehouse.s_instance._defaultClip;

			_stepSource = _sources[0];
			_behaviorSource = _sources[1];
			_voiceSource = _sources[2];
		}

		void Update()
		{

		}

		public void PlayStep(ClipsPawn _clip)
		{
			_stepSource.clip = AudioWarehouse.s_instance.Clip(_clip);
			_stepSource.Play();
		}

		public void PlayBehavior(ClipsPawn _clip)
		{
			_behaviorSource.clip = AudioWarehouse.s_instance.Clip(_clip);
			_behaviorSource.Play();
		}

		public void PlayVoice(ClipsPawn _clip)
		{
			_voiceSource.clip = AudioWarehouse.s_instance.Clip(_clip);
			_voiceSource.Play();
		}
	}
}