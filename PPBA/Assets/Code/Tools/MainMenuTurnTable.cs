using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class MainMenuTurnTable : MonoBehaviour
	{
		[SerializeField] float _timeBetweenJups;
		[SerializeField] float _translation;
		[SerializeField] AnimationCurve _jump;

		float _jumpAngle;

		void Start()
		{
			_jumpAngle = 360.0f / transform.childCount;

			for(int i = 0; i < transform.childCount; i++)
			{
				Transform it = transform.GetChild(i);
				it.rotation = Quaternion.Euler(0, _jumpAngle / 2 * i, 0);
				it.Translate(it.forward * _translation);
				/*Animation anim = it.gameObject.AddComponent<Animation>();
				anim.clip = _rotation;
				anim.Stop();
				anim.Play();*/
			}

			transform.Translate(transform.forward * -_translation);
		}

		float h_lastJump = 0;
		bool h_inJump = false;
		float h_startAngle = 0;
		// Update is called once per frame
		void Update()
		{

			if(h_inJump || Time.time > h_lastJump + _timeBetweenJups)
			{
				if(!h_inJump)
				{
					h_inJump = true;
					h_lastJump = Time.time;
					h_startAngle = transform.rotation.eulerAngles.y;
				}

				float value = _jump.Evaluate(Time.time - h_lastJump);
				transform.rotation = Quaternion.Euler(0, h_startAngle + value * _jumpAngle, 0);
				if(value > 0.999f)
				{
					h_inJump = false;
					transform.rotation = Quaternion.Euler(0, h_startAngle + _jumpAngle, 0);
				}
			}


		}
	}
}
