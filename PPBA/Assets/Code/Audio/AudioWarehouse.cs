using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public enum ClipsPawn
	{
		DEFAULT,
		GO_TO_BORDER_01,
		GO_TO_COVER_01,
		VOICE_SCREAM_DEATH_01,
		BUILD_REPAIR_01,
		DECONSTRUCT_01,
		EXPLOSION_01,
		SHOT_MP_01,
		VOICE_ATTACK_01,
		VOICE_BRINGSUPPLIES_01,
		VOICE_BUILD_ONIT_01,
		VOICE_CLICK_UNIT_01,
		VOICE_CONQUERBUILDING_01,
		VOICE_DECONSTRUCT_01,
		VOICE_FOLLOW_01,
		VOICE_GETAMMO_01,
		VOICE_GETSUPPLIES_01,
		VOICE_GOTOFLAG_IDLE_01,
		VOICE_GOTOHEAL_01,
		VOICE_MOUNT_BUILDING_01,
		VOICE_SCREAM_FLEE_01,
		VOICE_SCREAM_INPAIN_01,
		VOICE_STAYINCOVER_01,
		VOICE_THROWGRANADE_01,
		VOICE_UNIT_IDLE_BEHAVIOR_01,
		VOICE_WIN_CHEER_01,
	}

	public enum ClipsBuilding
	{
		DEFAULT,
		CLICK_AMMOFACTORY_IDLE_01,
		CLICK_BARBEDWIRE_01,
		CLICK_BARBEDWIRE_INDIVIDUAL_01,
		CLICK_DEPOT_IDLE_01,
		CLICK_HQ_IDLE_01,
		CLICK_MEDTEND_IDLE_01,
		CLICK_MORTAR_IDLE_01,
		CLICK_REFINERY_IDLE_01,
		CLICK_ROCKWALL_IDLE_01,
		CLICK_STREET_IDLE_01,
		CLICK_TERRAWALL_01,
		CLICK_TRASHWALL_IDLE_01,
		CLICK_TRENCH_01,
		CLICK_WATCHTOWER_IDLE_01,
		DESTROY_BUILDING_01,
		ERROR_PLACE_BUILDING_01,
		PLACE_BUILDING_01,
		PLACE_FLAG_01,
		SHOT_MG_01,
		SHOT_MG_NEST_01,
		SHOT_MORTAR_01,
		SHOT_MORTAR_02,
		UNIT_PRODUCED_01,
	}

	public enum ClipsUI
	{
		DEFAULT,
		BUTTON_BACK_01,
		BUTTON_CLICK_01,
		BUTTON_CLOSE_BACK_01,
		BUTTON_HOVER_01,
		BUTTON_PING_01,
		BUTTON_SELECTMINIMAP_01,
		ICON_CLICK_01,
		ICON_HOVER_01,
	}

	public enum ClipsMusic
	{
		DEFAULT,
		MUSIC_ENDSCREEN_LOSE_01,
		MUSIC_ENDSCREEN_WIN_01,
		MUSIC_TRACK_01,
	}

	public enum ClipsEnvironment
	{
		DEFAULT,
		ENVIRONMENT_BIRDS_01,
		ENVIRONMENT_ICE_CRACKING_01,
		ENVIRONMENT_LIGHT_SNOWSTORM_01,
		ENVIRONMENT_RIVER_01,
		ENVIRONMENT_WAR_01,
	}

	public class AudioWarehouse : Singleton<AudioWarehouse>
	{
		#region Name Arrays
		public ClipsPawn[] _namesPawn = new ClipsPawn[] {
			ClipsPawn.DEFAULT,
			ClipsPawn.GO_TO_BORDER_01,
			ClipsPawn.GO_TO_COVER_01,
			ClipsPawn.VOICE_SCREAM_DEATH_01,
			ClipsPawn.BUILD_REPAIR_01,
			ClipsPawn.DECONSTRUCT_01,
			ClipsPawn.EXPLOSION_01,
			ClipsPawn.SHOT_MP_01,
			ClipsPawn.VOICE_ATTACK_01,
			ClipsPawn.VOICE_BRINGSUPPLIES_01,
			ClipsPawn.VOICE_BUILD_ONIT_01,
			ClipsPawn.VOICE_CLICK_UNIT_01,
			ClipsPawn.VOICE_CONQUERBUILDING_01,
			ClipsPawn.VOICE_DECONSTRUCT_01,
			ClipsPawn.VOICE_FOLLOW_01,
			ClipsPawn.VOICE_GETAMMO_01,
			ClipsPawn.VOICE_GETSUPPLIES_01,
			ClipsPawn.VOICE_GOTOFLAG_IDLE_01,
			ClipsPawn.VOICE_GOTOHEAL_01,
			ClipsPawn.VOICE_MOUNT_BUILDING_01,
			ClipsPawn.VOICE_SCREAM_FLEE_01,
			ClipsPawn.VOICE_SCREAM_INPAIN_01,
			ClipsPawn.VOICE_STAYINCOVER_01,
			ClipsPawn.VOICE_THROWGRANADE_01,
			ClipsPawn.VOICE_UNIT_IDLE_BEHAVIOR_01,
			ClipsPawn.VOICE_WIN_CHEER_01,
		};

		public ClipsBuilding[] _namesBuilding = new ClipsBuilding[] {
			ClipsBuilding.DEFAULT,
			ClipsBuilding.CLICK_AMMOFACTORY_IDLE_01,
			ClipsBuilding.CLICK_BARBEDWIRE_01,
			ClipsBuilding.CLICK_BARBEDWIRE_INDIVIDUAL_01,
			ClipsBuilding.CLICK_DEPOT_IDLE_01,
			ClipsBuilding.CLICK_HQ_IDLE_01,
			ClipsBuilding.CLICK_MEDTEND_IDLE_01,
			ClipsBuilding.CLICK_MORTAR_IDLE_01,
			ClipsBuilding.CLICK_REFINERY_IDLE_01,
			ClipsBuilding.CLICK_ROCKWALL_IDLE_01,
			ClipsBuilding.CLICK_STREET_IDLE_01,
			ClipsBuilding.CLICK_TERRAWALL_01,
			ClipsBuilding.CLICK_TRASHWALL_IDLE_01,
			ClipsBuilding.CLICK_TRENCH_01,
			ClipsBuilding.CLICK_WATCHTOWER_IDLE_01,
			ClipsBuilding.DESTROY_BUILDING_01,
			ClipsBuilding.ERROR_PLACE_BUILDING_01,
			ClipsBuilding.PLACE_BUILDING_01,
			ClipsBuilding.PLACE_FLAG_01,
			ClipsBuilding.SHOT_MG_01,
			ClipsBuilding.SHOT_MG_NEST_01,
			ClipsBuilding.SHOT_MORTAR_01,
			ClipsBuilding.SHOT_MORTAR_02,
			ClipsBuilding.UNIT_PRODUCED_01,
		};

		public ClipsUI[] _namesUI = new ClipsUI[] {
			ClipsUI.DEFAULT,
			ClipsUI.BUTTON_BACK_01,
			ClipsUI.BUTTON_CLICK_01,
			ClipsUI.BUTTON_CLOSE_BACK_01,
			ClipsUI.BUTTON_HOVER_01,
			ClipsUI.BUTTON_PING_01,
			ClipsUI.BUTTON_SELECTMINIMAP_01,
			ClipsUI.ICON_CLICK_01,
			ClipsUI.ICON_HOVER_01,
		};

		public ClipsMusic[] _namesMusic = new ClipsMusic[] {
			ClipsMusic.DEFAULT,
			ClipsMusic.MUSIC_ENDSCREEN_LOSE_01,
			ClipsMusic.MUSIC_ENDSCREEN_WIN_01,
			ClipsMusic.MUSIC_TRACK_01,
		};

		public ClipsEnvironment[] _namesEnvironment = new ClipsEnvironment[] {
			ClipsEnvironment.DEFAULT,
			ClipsEnvironment.ENVIRONMENT_BIRDS_01,
			ClipsEnvironment.ENVIRONMENT_ICE_CRACKING_01,
			ClipsEnvironment.ENVIRONMENT_LIGHT_SNOWSTORM_01,
			ClipsEnvironment.ENVIRONMENT_RIVER_01,
			ClipsEnvironment.ENVIRONMENT_WAR_01,
		};
		#endregion
		
		public static Dictionary<ClipsPawn, AudioClip> _pawnClipDict = new Dictionary<ClipsPawn, AudioClip>();
		public static Dictionary<ClipsBuilding, AudioClip> _buildingClipDict = new Dictionary<ClipsBuilding, AudioClip>();
		public static Dictionary<ClipsUI, AudioClip> _uiClipDict = new Dictionary<ClipsUI, AudioClip>();
		public static Dictionary<ClipsMusic, AudioClip> _musicClipDict = new Dictionary<ClipsMusic, AudioClip>();
		public static Dictionary<ClipsEnvironment, AudioClip> _environmentClipDict = new Dictionary<ClipsEnvironment, AudioClip>();
		
		public AudioClip _defaultClip;

		void Awake()
		{
			LoadAll();
		}

		void Start()
		{

		}

		public AudioClip Clip(ClipsPawn effect) => _pawnClipDict.ContainsKey(effect) ? _pawnClipDict[effect] : _defaultClip;
		public AudioClip Clip(ClipsBuilding effect) => _buildingClipDict.ContainsKey(effect) ? _buildingClipDict[effect] : _defaultClip;
		public AudioClip Clip(ClipsUI effect) => _uiClipDict.ContainsKey(effect) ? _uiClipDict[effect] : _defaultClip;
		public AudioClip Clip(ClipsMusic effect) => _musicClipDict.ContainsKey(effect) ? _musicClipDict[effect] : _defaultClip;
		public AudioClip Clip(ClipsEnvironment effect) => _environmentClipDict.ContainsKey(effect) ? _environmentClipDict[effect] : _defaultClip;

		public void LoadAll()
		{
			foreach(ClipsPawn item in _namesPawn)
			{
				string path = "ClipsPawn/" + item.ToString();
				AudioClip temp = Resources.Load<AudioClip>(path);
				_pawnClipDict[item] = null != temp ? temp : _defaultClip;
			}

			foreach(ClipsBuilding item in _namesPawn)
			{
				string path = "ClipsBuilding/" + item.ToString();
				AudioClip temp = Resources.Load<AudioClip>(path);
				_buildingClipDict[item] = null != temp ? temp : _defaultClip;
			}

			foreach(ClipsUI item in _namesPawn)
			{
				string path = "ClipsUI/" + item.ToString();
				AudioClip temp = Resources.Load<AudioClip>(path);
				_uiClipDict[item] = null != temp ? temp : _defaultClip;
			}

			foreach(ClipsMusic item in _namesPawn)
			{
				string path = "ClipsMusic/" + item.ToString();
				AudioClip temp = Resources.Load<AudioClip>(path);
				_musicClipDict[item] = null != temp ? temp : _defaultClip;
			}

			foreach(ClipsEnvironment item in _namesEnvironment)
			{
				string path = "ClipsEnvironment/" + item.ToString();
				AudioClip temp = Resources.Load<AudioClip>(path);
				_environmentClipDict[item] = null != temp ? temp : _defaultClip;
			}
		}
	}
}
