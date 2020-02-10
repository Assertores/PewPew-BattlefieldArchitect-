using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class MovableSpeakerController : MonoBehaviour
	{
		[SerializeField] private static AudioSource[] _speakers = new AudioSource[0];
		private static int _ticker = 0;

		void Start()
		{
			_speakers = GetComponentsInChildren<AudioSource>();
		}

		private static AudioSource GetNextSource() => _speakers[++_ticker % _speakers.Length];

		public static void PlaySoundAtSpot(AudioClip audioClip, Vector3 spot)
		{
			AudioSource source = GetNextSource();
			source.transform.position = spot;
			source.PlayOneShot(audioClip);
		}
	}
}