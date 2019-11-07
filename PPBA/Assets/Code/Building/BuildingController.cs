﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PPBA
{
	public class BuildingController : Singleton<BuildingController>
	{
		private GameObject _currentPlaceableObject;
		private GameObject _currentBetweenObject;
		private GameObject _curItem;

		private float _mouseWheelRotation;
		private int _currentPrefabIndex = 0;
		private bool _canConnect = false;

		Vector2 _pixelUV = Vector2.zero;

		private GameObject lastPole;

		private void Update()
		{
			if(_currentPlaceableObject != null)
			{
				CancelBuilding();

				if(_currentPrefabIndex == 0)
				{
					MoveCurrentObjectToMouse();
					RotateFromMouseWheel();

				}
				else if(_curItem != null && _canConnect)
				{
					CreateSegment();
				}

				ReleaseIfClicked();
			}
		}

		private void ReleaseIfClicked()
		{
			if(Input.GetMouseButtonDown(0))
			{
				if(!_canConnect)
				{
			//		ResourceMapCalculate.AddRefinery(_curItem.GetComponent<RefineryRefHolder>());

					float radius = _curItem.GetComponent<RefineryRefHolder>()._harvestRadius;
					float intensity = _curItem.GetComponent<RefineryRefHolder>()._harvestIntensity;

					//GetComponent<RessourceManager>().AddRefinery(curItem.prefab.GetComponent<RefineryScript>());
					// to do in constructBuild einfügen
		//			ResourceMapChanger.instance.AddFabric(new Vector3(_pixelUV.x, 0, _pixelUV.y), intensity, radius);
					_currentPlaceableObject = null;
					_curItem = null;
					return;
				}
				ConstructBuild();
				_currentPrefabIndex++;
				_currentBetweenObject = Instantiate(_currentBetweenObject);

			}
		}

		public float angle = 0;
		private void CreateSegment()
		{
			Vector3 dir = (UserInputController.s_instance.GetWorldPoint() - lastPole.transform.position);
			float dis = dir.magnitude;

			//if(dis < 1)
			//{
			//	return;
			//}

			float length = _currentBetweenObject.GetComponent<MeshRenderer>().bounds.size.z;

			Vector3 v3Pos = Camera.main.WorldToScreenPoint(lastPole.transform.position);
			v3Pos = Input.mousePosition - v3Pos;
			angle = Mathf.Atan2(v3Pos.y, v3Pos.x) * Mathf.Rad2Deg;
			v3Pos = Quaternion.AngleAxis(-angle, Vector3.up) * (Vector3.back * length);
			_currentPlaceableObject.transform.position = lastPole.transform.position + v3Pos;


			Vector3 dir2 = (_currentPlaceableObject.transform.position - lastPole.transform.position);
			Vector3 pos = dir2 * 0.5f + lastPole.transform.position;
			Quaternion rotationObj = Quaternion.LookRotation(dir2, Vector3.up);
			_currentBetweenObject.transform.position = pos;
			_currentBetweenObject.transform.rotation = rotationObj;

			if(dis > length)
			{
				ConstructBetween();
				ConstructBuild();
			}
		}

		private void ConstructBuild()
		{
			lastPole = _currentPlaceableObject;
			_currentPlaceableObject = null;
			_currentPlaceableObject = Instantiate(_curItem);
		}

		private void ConstructBetween()
		{
			Vector3 dir2 = (_currentPlaceableObject.transform.position - lastPole.transform.position);
			Vector3 pos = dir2 * 0.5f + lastPole.transform.position;
			Quaternion rotationObj = Quaternion.LookRotation(dir2, Vector3.up);

			//	Instantiate(_curItem.ConnectingObject, pos, rotationObj);

		}

		private void RotateFromMouseWheel()
		{
			_mouseWheelRotation += Input.mouseScrollDelta.y;
			_currentPlaceableObject.transform.Rotate(Vector3.up, _mouseWheelRotation * 10);
		}

		private void MoveCurrentObjectToMouse()
		{
			_currentPlaceableObject.transform.position = UserInputController.s_instance.GetWorldPoint();

			print(""+ UserInputController.s_instance.GetWorldPoint());
			//currentPlaceableObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);

		}

		public void HandleNewObject(GameObject PrefabBuildingType)
		{
			if(PrefabBuildingType.GetComponent<IUIElement>()._Type == ObjectType.REFINERY)
			{
				_curItem = PrefabBuildingType;
				_currentPlaceableObject = Instantiate(PrefabBuildingType.GetComponent<IUIElement>()._GhostPrefabObj);
				_canConnect = false;
			}

			if(PrefabBuildingType.GetComponent<IUIElement>()._Type == ObjectType.WALL)
			{
				_curItem = PrefabBuildingType;
				_currentPlaceableObject = Instantiate( PrefabBuildingType.GetComponent<IUIElement>()._GhostPrefabObj);
				_canConnect = true;
			}
		}

		public void CancelBuilding()
		{
			if(Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
			{
				_curItem = null;
				Destroy(_currentPlaceableObject);
				_currentPrefabIndex = 0;

			}
		}
	}
}

