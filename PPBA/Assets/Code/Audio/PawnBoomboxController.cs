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
				_sources[i].clip = AudioWarehouse._defaultClip;

			_stepSource = _sources[0];
			_behaviorSource = _sources[1];
			_voiceSource = _sources[2];
		}

		void Update()
		{

		}

		public void PlaySpawn() => _behaviorSource?.PlayOneShot(AudioWarehouse.s_instance.Clip(ClipsBuilding.UNIT_PRODUCED_01));

		public void PlayClick() => Random.Range(0, 1);//TODO

		public void PlayStep() => _stepSource?.PlayOneShot(AudioWarehouse.s_instance.Clip(Random.Range(0f, 1f) < 0.5f ? ClipsPawn.UNIT_MOVE_01 : ClipsPawn.UNIT_MOVE_02));

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

		public void PlayGetShot() => _behaviorSource?.PlayOneShot(AudioWarehouse.s_instance.Clip(Random.Range(0f, 1f) < 0.5f ? ClipsPawn.UNIT_HIT_SHOT_01 : ClipsPawn.UNIT_HIT_SHOT_02));
	}
}