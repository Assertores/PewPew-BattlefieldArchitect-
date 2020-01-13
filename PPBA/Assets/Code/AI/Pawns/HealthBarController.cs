using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PPBA
{
	public class HealthBarController : MonoBehaviour
	{
		[SerializeField] private Image _background;
		[SerializeField] private Image _healthBar;
		[SerializeField] private Image _moraleBar;
		[SerializeField] private Image _ammoBar;

		void Start()
		{

		}

		void Update()
		{
#if !UNITY_SERVER
			transform.LookAt(Camera.main.transform);
#endif
		}

		public void SetBars(float health, float morale, float ammo)
		{
			_healthBar.fillAmount = health;
			_moraleBar.fillAmount = morale;
			_ammoBar.fillAmount = ammo;
		}

		private void OnValidate()
		{
			if(null == _background)
				_background = transform.GetChild(0)?.GetComponent<Image>();
			if(null == _healthBar)
				_healthBar = transform.GetChild(1)?.GetComponent<Image>();
			if(null == _moraleBar)
				_moraleBar = transform.GetChild(2)?.GetComponent<Image>();
			if(null == _ammoBar)
				_ammoBar = transform.GetChild(3)?.GetComponent<Image>();
		}
	}
}