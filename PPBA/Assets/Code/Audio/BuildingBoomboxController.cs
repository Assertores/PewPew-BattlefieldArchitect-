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

		private bool wasDisabled = false;

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


		private void OnDisable()
		{
#if !UNITY_SERVER
			if(wasDisabled)
				MovableSpeakerController.PlaySoundAtSpot(AudioWarehouse.s_instance.Clip(ClipsBuilding.DESTROY_BUILDING_01), transform.position);
			else
				wasDisabled = true;
#endif
		}
	}
}