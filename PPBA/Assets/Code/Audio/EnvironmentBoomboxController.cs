using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	[RequireComponent(typeof(AudioSource))]
	public class EnvironmentBoomboxController : MonoBehaviour
	{
		[SerializeField] private ClipsEnvironment _clipName;
		private AudioSource _source;

		void Awake()
		{
			_source = GetComponent<AudioSource>();
		}

		void Start()
		{
			if(null == _source)
				return;

			_source.clip = AudioWarehouse.s_instance.Clip(_clipName);
			_source.Play();
		}

		void Update()
		{
			
		}
	}
}