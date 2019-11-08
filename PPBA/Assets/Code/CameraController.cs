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
	[SerializeField] Vector2 _panLimit = new Vector2(40f,40f);

	void Update()
	{
		if(Input.GetMouseButton(1))
		{
			 (new Vector3(0, Input.GetAxis("Mouse X") * _RotateSpeed, 0), Space.World);
			transform.rotation = Quaternion.Euler(0,,0);
		}

		Vector3 pos = transform.position;
		Vector3 mousePos = Input.mousePosition;

		if(Input.GetKey(KeyCode.W) || mousePos.y >= Screen.height - _BorderThickness)
		{
			pos.z += _CameraSpeed * Time.deltaTime;
		}
		if(Input.GetKey(KeyCode.S) || mousePos.y <= _BorderThickness)
		{
			pos.z -= _CameraSpeed * Time.deltaTime;
		}
		if(Input.GetKey(KeyCode.D) || mousePos.x >= Screen.width - _BorderThickness)
		{
			pos.x += _CameraSpeed * Time.deltaTime;
		}
		if(Input.GetKey(KeyCode.A) || mousePos.x <= _BorderThickness)
		{
			pos.x -= _CameraSpeed * Time.deltaTime;
		}

		pos.x = Mathf.Clamp(pos.x, -_panLimit.x, _panLimit.x);
		pos.z = Mathf.Clamp(pos.z, -_panLimit.y, _panLimit.y);

		Vector3 camF = Vector3.forward;
		Vector3 camR = Vector3.right;

		transform.position = (camF*pos.x+ camR*pos.y;
	}

}

