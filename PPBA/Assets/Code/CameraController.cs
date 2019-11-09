using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour
{
	CinemachineFreeLook cam;

	[SerializeField] float _CameraSpeed = 20f;
	[SerializeField] float _RotateSpeed = 20f;
	[SerializeField] float _BorderThickness = 10f;
	[SerializeField] Vector2 _panLimit = new Vector2(40f, 40f);

	void Update()
	{
		if(Input.GetMouseButton(1))
		{
			//(new Vector3(0, Input.GetAxis("Mouse X") * _RotateSpeed, 0), Space.World);
			transform.Rotate(new Vector3(0, Input.GetAxis("Mouse X") * _RotateSpeed, 0), Space.World);
		}

		Vector3 pos = Vector3.zero;
		Vector3 mousePos = Input.mousePosition;

		if(Input.GetKey(KeyCode.W) || mousePos.y >= Screen.height - _BorderThickness)
		{
			pos += _CameraSpeed * Vector3.forward * Time.deltaTime;
		}
		if(Input.GetKey(KeyCode.S) || mousePos.y <= _BorderThickness)
		{
			pos += _CameraSpeed * Vector3.back * Time.deltaTime;
		}
		if(Input.GetKey(KeyCode.D) || mousePos.x >= Screen.width - _BorderThickness)
		{
			pos += _CameraSpeed * Vector3.right * Time.deltaTime;
		}
		if(Input.GetKey(KeyCode.A) || mousePos.x <= _BorderThickness)
		{
			pos += _CameraSpeed * Vector3.left * Time.deltaTime;
		}
		transform.Translate(pos, Space.Self);

		//pos.x = Mathf.Clamp(transform.position.x, -_panLimit.x, _panLimit.x);
		//pos.z = Mathf.Clamp(transform.position.z, -_panLimit.y, _panLimit.y);

	}

}

