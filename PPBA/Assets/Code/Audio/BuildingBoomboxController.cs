using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	[RequireComponent(typeof(AudioSource))]
	public class BuildingBoomboxController : MonoBehaviour
	{
		private AudioSource _source;

		[SerializeField] private ClipsBuilding _clickClip;

		void Awake()
		{
			_source = GetComponent<AudioSource>();
		}

		void Start()
		{

		}

		void Update()
		{

		}

		public void PlayClickSound() => _source.PlayOneShot(AudioWarehouse.s_instance.Clip(_clickClip));

		public void PlaySound(ClipsBuilding _clipName) => _source.PlayOneShot(AudioWarehouse.s_instance.Clip(_clipName));
	}
}