﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

namespace PPBA
{


	public class CameraController : MonoBehaviour
	{
		CinemachineFreeLook cam;

		[SerializeField] float _CameraSpeed = 20f;
		[SerializeField] float _RotateSpeed = 20f;
		//[SerializeField] float _BorderThickness = 10f;
		[SerializeField] Vector2 _panLimit = new Vector2(450, 450f);


		void LateUpdate()
		{
			if(Input.GetMouseButton(1))
			{
				transform.Rotate(new Vector3(0, Input.GetAxis("Mouse X") * _RotateSpeed, 0), Space.World);
			}

			Vector3 pos = Vector3.zero;
			Vector3 mousePos = Input.mousePosition;

			if(Input.GetKey(KeyCode.W) /*|| mousePos.y >= Screen.height - _BorderThickness*/)
			{
				pos += _CameraSpeed * Vector3.forward * Time.deltaTime;
			}
			if(Input.GetKey(KeyCode.S) /*|| mousePos.y <= _BorderThickness*/)
			{
				pos += _CameraSpeed * Vector3.back * Time.deltaTime;
			}
			if(Input.GetKey(KeyCode.D) /*|| mousePos.x >= Screen.width - _BorderThickness*/)
			{
				pos += _CameraSpeed * Vector3.right * Time.deltaTime;
			}
			if(Input.GetKey(KeyCode.A) /*|| mousePos.x <= _BorderThickness*/)
			{
				pos += _CameraSpeed * Vector3.left * Time.deltaTime;
			}

			//print(pos);
			pos.x += transform.position.x;
			pos.z += transform.position.z;

			pos.x = Mathf.Clamp(pos.x, -_panLimit.x, _panLimit.x);
			pos.z = Mathf.Clamp(pos.z, -_panLimit.y, _panLimit.y);

			transform.position = pos;
		}

	}
}
