using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public enum ClipsPawn
	{
		DEFAULT,
	}

	public enum ClipsBuilding
	{
		DEFAULT,
	}

	public enum ClipsUI
	{
		DEFAULT,
	}

	public enum ClipsMusic
	{
		DEFAULT,
	}

	public class AudioWarehouse : Singleton<AudioWarehouse>
	{
		public ClipsPawn[] _namesPawn = new ClipsPawn[] { ClipsPawn.DEFAULT };
		public ClipsBuilding[] _namesBuilding = new ClipsBuilding[] { ClipsBuilding.DEFAULT };
		public ClipsUI[] _namesUI = new ClipsUI[] { ClipsUI.DEFAULT };
		public ClipsMusic[] _namesMusic = new ClipsMusic[] { ClipsMusic.DEFAULT };

		public static Dictionary<ClipsPawn, AudioClip> _pawnClipDict = new Dictionary<ClipsPawn, AudioClip>();
		public static Dictionary<ClipsBuilding, AudioClip> _buildingClipDict = new Dictionary<ClipsBuilding, AudioClip>();
		public static Dictionary<ClipsUI, AudioClip> _uiClipDict = new Dictionary<ClipsUI, AudioClip>();
		public static Dictionary<ClipsMusic, AudioClip> _musicClipDict = new Dictionary<ClipsMusic, AudioClip>();

		public AudioClip _defaultClip;

		void Awake()
		{
			//LoadAll();
		}

		void Start()
		{

		}

		public AudioClip Clip(ClipsPawn effect) => _pawnClipDict.ContainsKey(effect) ? _pawnClipDict[effect] : _defaultClip;
		public AudioClip Clip(ClipsBuilding effect) => _buildingClipDict.ContainsKey(effect) ? _buildingClipDict[effect] : _defaultClip;
		public AudioClip Clip(ClipsUI effect) => _uiClipDict.ContainsKey(effect) ? _uiClipDict[effect] : _defaultClip;
		public AudioClip Clip(ClipsMusic effect) => _musicClipDict.ContainsKey(effect) ? _musicClipDict[effect] : _defaultClip;

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
		}
	}
}
