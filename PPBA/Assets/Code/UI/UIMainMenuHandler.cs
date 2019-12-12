using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;

namespace PPBA
{
	public class UIMainMenuHandler : MonoBehaviour
	{

		#region Variables

		[SerializeField] private TMP_InputField _ipField;
		[SerializeField] private TMP_InputField _portField;
		[SerializeField] private GameObject _startPannel;
		[SerializeField] private GameObject _optionsPannel;
		[SerializeField] private GameObject _controlsPannel;
		[SerializeField] private GameObject _creditsPannel;
		[SerializeField] private GameObject _soundsPannel;
		[SerializeField] private GameObject _graphicsPannel;
		[SerializeField] private AudioMixer _mixer;
		[SerializeField] private Slider _masterSlider;
		[SerializeField] private Slider _musicSlider;
		[SerializeField] private Slider _sfxSlider;
		[SerializeField] private PostProcessProfile _profile;
		private ColorGrading _colorGrading;
		[SerializeField] private Slider _brightnessSlider;
		[SerializeField] private Slider _contrastSlider;
		[SerializeField] private TextMeshProUGUI _resolution;
		[SerializeField] private TextMeshProUGUI _fullscreen;
		[SerializeField] private string[] _fullscreenTexts;

		#endregion
		#region MonoBehaviour

		private void Start()
		{
#if UNITY_SERVER
			Destroy(this.gameObject);
			return;
#endif
			#region Checks
			if(!_ipField)
			{
				Debug.LogError("ipField reference not set");
				Destroy(this);
				return;
			}
			if(!_portField)
			{
				Debug.LogError("portField reference not set");
				Destroy(this);
				return;
			}
			if(!_startPannel)
			{
				Debug.LogError("startPannel reference not set");
				Destroy(this);
				return;
			}
			if(!_optionsPannel)
			{
				Debug.LogError("optionsPannel reference not set");
				Destroy(this);
				return;
			}
			if(!_controlsPannel)
			{
				Debug.LogError("controlsPannel reference not set");
				Destroy(this);
				return;
			}
			if(!_creditsPannel)
			{
				Debug.LogError("creditsPannel reference not set");
				Destroy(this);
				return;
			}
			if(!_soundsPannel)
			{
				Debug.LogError("soundsPannel reference not set");
				Destroy(this);
				return;
			}
			if(!_graphicsPannel)
			{
				Debug.LogError("graphicsPannel reference not set");
				Destroy(this);
				return;
			}
			if(!_mixer)
			{
				Debug.LogError("Audiomixer reference not set");
				Destroy(this);
				return;
			}
			if(!_masterSlider)
			{
				Debug.LogError("Slider for Master Volume reference not set");
				Destroy(this);
				return;
			}
			if(!_musicSlider)
			{
				Debug.LogError("Slider for Music Volume reference not set");
				Destroy(this);
				return;
			}
			if(!_sfxSlider)
			{
				Debug.LogError("Slider for Sound Effects Volume reference not set");
				Destroy(this);
				return;
			}
			if(!_profile)
			{
				Debug.LogError("Post processing profile reference not set");
				Destroy(this);
				return;
			}
			if(!_brightnessSlider)
			{
				Debug.LogError("Slider for Brightness reference not set");
				Destroy(this);
				return;
			}
			if(!_contrastSlider)
			{
				Debug.LogError("Slider for Contrast reference not set");
				Destroy(this);
				return;
			}
			if(!_resolution)
			{
				Debug.LogError("Text for resolution reference not set");
				Destroy(this);
				return;
			}
			if(!_fullscreen)
			{
				Debug.LogError("Text for fullscreen reference not set");
				Destroy(this);
				return;
			}
			if(_fullscreenTexts.Length < 2)
			{
				Debug.LogError("Not enough text for fullscreen texts");
				Destroy(this);
				return;
			}
			#endregion

			float tmp;
			_mixer.GetFloat(StringCollection.MASTER, out tmp);
			_masterSlider.value = Mathf.InverseLerp(-30, 20, tmp) * 10;

			_mixer.GetFloat(StringCollection.MUSIC, out tmp);
			_musicSlider.value = Mathf.InverseLerp(-30, 20, tmp) * 10;

			_mixer.GetFloat(StringCollection.SFX, out tmp);
			_sfxSlider.value = Mathf.InverseLerp(-30, 20, tmp) * 10;


			_profile.TryGetSettings(out _colorGrading);

			int i = 0;
			for(; i < Screen.resolutions.Length && (Screen.resolutions[i].width != Screen.currentResolution.width || Screen.resolutions[i].height != Screen.currentResolution.height)/*refresh time*/; i++)
				;
			h_currentResolutionIndex = i;
			Resolution res = Screen.resolutions[h_currentResolutionIndex];
			_resolution.text = res.width + " x " + res.height;

			_brightnessSlider.value = Mathf.RoundToInt((_colorGrading.gamma.value.w + 1) * 5);
			_contrastSlider.value = Mathf.RoundToInt((_colorGrading.contrast.value + 100) / 20);

			ChangeFullscreen(Screen.fullScreen);

			DeactivateAllPannels();
		}

		#endregion
		#region Pannel

		public void ToggleStartPannel()
		{
			bool tmp = !_startPannel.activeSelf;
			DeactivateAllPannels();
			_startPannel.SetActive(tmp);
		}

		public void ToggleOptionsPannel()
		{
			bool tmp = !_optionsPannel.activeSelf;
			DeactivateAllPannels();
			_optionsPannel.SetActive(tmp);
		}

		public void ToggleControlsPannel()
		{
			bool tmp = !_controlsPannel.activeSelf;
			DeactivateAllPannels();
			_controlsPannel.SetActive(tmp);
		}

		public void ToggleCreditsPannel()
		{
			bool tmp = !_creditsPannel.activeSelf;
			DeactivateAllPannels();
			_creditsPannel.SetActive(tmp);
		}

		public void ToggleSoundsPannel()
		{
			bool tmp = !_soundsPannel.activeSelf;
			DeactivateAllPannels();
			_optionsPannel.SetActive(true);
			_soundsPannel.SetActive(tmp);
		}

		public void ToggleGraphicsPannel()
		{
			bool tmp = !_graphicsPannel.activeSelf;
			DeactivateAllPannels();
			_optionsPannel.SetActive(true);
			_graphicsPannel.SetActive(tmp);
		}

		void DeactivateAllPannels()
		{
			_startPannel.SetActive(false);
			_optionsPannel.SetActive(false);
			_controlsPannel.SetActive(false);
			_creditsPannel.SetActive(false);
			_soundsPannel.SetActive(false);
			_graphicsPannel.SetActive(false);
		}

		#endregion

		public void ConnectToServer()
		{
			Debug.Log("Change Scene");
			string ip = _ipField.text == "" ? "127.0.0.1" : _ipField.text;
			int port = _portField.text == "" ? 13370 : int.Parse(_portField.text);
			GameNetcode.s_instance.ClientConnect(ip, port);
		}

		public void Quit()
		{
			Application.Quit();
#if UNITY_EDITOR
			Debug.Break();
#endif
		}

		#region Volume

		public void ChangeMasterVolume(float value)
		{
			_mixer.SetFloat(StringCollection.MASTER, Mathf.Lerp(-30, 20, value / 10));
		}

		public void ChangeMusicVolume(float value)
		{
			_mixer.SetFloat(StringCollection.MUSIC, Mathf.Lerp(-30, 20, value / 10));
		}

		public void ChangeEffectsVolume(float value)
		{
			_mixer.SetFloat(StringCollection.SFX, Mathf.Lerp(-30, 20, value / 10));
		}

		#endregion
		#region Graphics

		public void ChangeBrightness(float value)
		{// from 0 to 10
			_colorGrading.gamma.value.w = value / 5 - 1;
		}

		public void ChangeContrast(float value)
		{// from 0 to 10
			_colorGrading.contrast.value = value * 20 - 100;
		}

		int h_currentResolutionIndex;
		public void ChangeResolution(bool next)
		{
			h_currentResolutionIndex += next ? 1 : -1;
			if(h_currentResolutionIndex < 0)
				h_currentResolutionIndex = 0;
			if(h_currentResolutionIndex >= Screen.resolutions.Length)
				h_currentResolutionIndex = Screen.resolutions.Length - 1;

			Resolution tmp = Screen.resolutions[h_currentResolutionIndex];
			Screen.SetResolution(tmp.width, tmp.height, Screen.fullScreen);
			_resolution.text = tmp.width + " x " + tmp.height;
		}

		public void ChangeFullscreen(bool next)
		{
			Screen.fullScreen = next;
			_fullscreen.text = _fullscreenTexts[next ? 1 : 0];
		}

		#endregion
	}
}
