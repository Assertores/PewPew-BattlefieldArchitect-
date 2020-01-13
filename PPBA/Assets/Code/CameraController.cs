﻿using UnityEngine;

namespace PPBA
{


	public class CameraController : MonoBehaviour
	{
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
				//pos += _CameraSpeed * Vector3.forward * Time.deltaTime;
				//pos += _CameraSpeed * Camera.main.transform.forward * Time.deltaTime;
				pos += ((Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up)).normalized * Time.deltaTime) * (_CameraSpeed);
			}
			if(Input.GetKey(KeyCode.S) /*|| mousePos.y <= _BorderThickness*/)
			{
				//pos += _CameraSpeed * -Camera.main.transform.forward * Time.deltaTime;
				pos += ((Vector3.ProjectOnPlane(-Camera.main.transform.forward, Vector3.up)).normalized * Time.deltaTime) * (_CameraSpeed);

			}
			if(Input.GetKey(KeyCode.D) /*|| mousePos.x >= Screen.width - _BorderThickness*/)
			{
				//pos += _CameraSpeed * Camera.main.transform.right * Time.deltaTime;
				pos += ((Vector3.ProjectOnPlane(Camera.main.transform.right, Vector3.up)).normalized * Time.deltaTime) * (_CameraSpeed );

			}
			if(Input.GetKey(KeyCode.A) /*|| mousePos.x <= _BorderThickness*/)
			{
				//pos += _CameraSpeed * -Camera.main.transform.right* Time.deltaTime;
				pos += ((Vector3.ProjectOnPlane(-Camera.main.transform.right, Vector3.up)).normalized * Time.deltaTime) * (_CameraSpeed );

			}

			pos.x += transform.position.x;
			pos.z += transform.position.z;

			pos.x = Mathf.Clamp(pos.x, -_panLimit.x, _panLimit.x);
			pos.z = Mathf.Clamp(pos.z, -_panLimit.y, _panLimit.y);

			transform.position = pos ;
		}

	}
}
