using System.Collections.Generic;
using UnityEngine;

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
			if(/*Input.GetMouseButton(1)*/ Input.GetKey(KeyCode.E))
			{
				transform.Rotate(new Vector3(0, 1 * _RotateSpeed, 0), Space.World);
			}

			if(/*Input.GetMouseButton(1)*/ Input.GetKey(KeyCode.Q))
			{
				transform.Rotate(new Vector3(0, -1 * _RotateSpeed, 0), Space.World);
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

			if(Input.GetKeyDown(KeyCode.H))
			{
				if(BuildingManager.s_instance._HQ.Count > 0)
				{
					IRefHolder holder = BuildingManager.s_instance._HQ[0];
					transform.position = ((MonoBehaviour)holder).transform.position;
				}

				//foreach(KeyValuePair<GameObject, ObjectType> build in BuildingManager.s_instance._HQ)
				//{
				//	if(ObjectType.HQ == build.Value)
				//	{
				//	print("jump!!!!!!!!!!!!!!!");
				//		transform.position = build.Key.transform.position;
				//		break;
				//	}
				//}
			}
		}
	}
}
