using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class BuildingManager : Singleton<BuildingManager>
	{
		public Color _ghostGreenColor;
		public Color _ghostRedColor;

		public GameObject _ghostWall;
		public GameObject _ghostWallBetween;

		private ObjectType _currentObjectType;
		private GameObject _currentPlaceableObject;
		private GameObject _currentBetweenObject;
		private Vector3 _lastPole;
		private bool _isBuilt = false;

		private Dictionary<GameObject, ObjectType> _placedBuiltings = new Dictionary<GameObject, ObjectType>();

		public bool _canBuild = true;

		public float _mouseWheelRotation;
		private int _currentPrefabIndex = 0;

		private void Update()
		{
			if(_currentPlaceableObject != null)
			{
				if(Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
				{
					CancelBuilding();
					return;
				}

				//if(!_isBuilt)
				//{
				MoveCurrentObjectToMouse();
				RotateCurrentObjectWithMouseWheel();

				if(Input.GetMouseButtonDown(0))
				{
					if(_canBuild)
					{
						ReleaseIfClicked();
					}
				}
				//}

				if(Input.GetMouseButton(0) && _currentPrefabIndex > 0)
				{
					WallBuildingRoutine();
				}

				if(Input.GetMouseButtonUp(0) && _isBuilt && _currentPrefabIndex > 2)
				{
					if(_placedBuiltings.ContainsKey(_currentPlaceableObject))
					{
						_placedBuiltings.Remove(_currentPlaceableObject);
					}

					if(!_canBuild)
					{   // for the last one with this we can go on building
						print("!!!_canBuild");
						foreach(KeyValuePair<GameObject, ObjectType> build in _placedBuiltings)
						{
							Destroy(build.Key);
						}
					}
					else
					{
						foreach(KeyValuePair<GameObject, ObjectType> build in _placedBuiltings)
						{
							build.Key.GetComponent<TickBuildEmitter>().AddToGatherValue();
						}
					}

					Destroy(_currentPlaceableObject);
					EndBuilding();
				}
			}
		}

		private void ReleaseIfClicked()
		{
			if(_currentObjectType != ObjectType.WALL)
			{
				PlaceRefineryPrefab();
			}

			if(_currentObjectType == ObjectType.WALL)
			{
				ConstructWall();
				//_currentBetweenObject = Instantiate(_ghostWallBetween, UserInputController.s_instance.GetWorldPoint(), Quaternion.identity);
				ConstructBetween();

			}
		}

		public void HandleNewObject(IRefHolder PrefabBuildingType)
		{
			CancelBuilding();
			_currentObjectType = PrefabBuildingType._Type;

			switch(PrefabBuildingType._Type)
			{
				case ObjectType.REFINERY:
					_currentPlaceableObject = Instantiate(PrefabBuildingType._GhostPrefabObj, UserInputController.s_instance.GetWorldPoint(), Quaternion.identity);
					break;
				case ObjectType.DEPOT:
					_currentPlaceableObject = Instantiate(PrefabBuildingType._GhostPrefabObj, UserInputController.s_instance.GetWorldPoint(), Quaternion.identity);
					break;
				case ObjectType.GUN_TURRET:
					break;
				case ObjectType.WALL:
					ConstructWall();
					break;
				case ObjectType.COVER:
					_currentPlaceableObject = Instantiate(PrefabBuildingType._GhostPrefabObj, UserInputController.s_instance.GetWorldPoint(), Quaternion.identity);
					break;
				case ObjectType.FLAGPOLE:
					_currentPlaceableObject = Instantiate(PrefabBuildingType._GhostPrefabObj, UserInputController.s_instance.GetWorldPoint(), Quaternion.identity);
					break;
				case ObjectType.HQ:
					_currentPlaceableObject = Instantiate(PrefabBuildingType._GhostPrefabObj, UserInputController.s_instance.GetWorldPoint(), Quaternion.identity);
					break;
				case ObjectType.MEDICAMP:
					_currentPlaceableObject = Instantiate(PrefabBuildingType._GhostPrefabObj, UserInputController.s_instance.GetWorldPoint(), Quaternion.identity);
					break;
				case ObjectType.SIZE:
					break;

			}
		}

		public void EndBuilding()
		{
			_currentPlaceableObject = null;
			_currentBetweenObject = null;
			_currentPrefabIndex = 0;
			_isBuilt = false;
			_placedBuiltings.Clear();
			_canBuild = true;
		}

		public void CancelBuilding()
		{
			Destroy(_currentPlaceableObject);

			if(_currentBetweenObject != null)
			{
				_currentBetweenObject = null;
			}

			_currentPrefabIndex = 0;
			_isBuilt = false;
			_canBuild = true;

			foreach(KeyValuePair<GameObject, ObjectType> build in _placedBuiltings)
			{
				Destroy(build.Key);
			}
			_placedBuiltings.Clear();
		}

		private void RotateCurrentObjectWithMouseWheel()
		{
			if(Input.GetKey(KeyCode.R))
			{
				//_MouseWheelRotation = Input.mouseScrollDelta.y;
				_currentPlaceableObject.transform.Rotate(Vector3.up, _mouseWheelRotation * Time.deltaTime);
			}
			if(Input.GetKey(KeyCode.T))
			{
				//_MouseWheelRotation = Input.mouseScrollDelta.y;
				_currentPlaceableObject.transform.Rotate(Vector3.up, -_mouseWheelRotation * Time.deltaTime);
			}
		}

		private void MoveCurrentObjectToMouse()
		{
			_currentPlaceableObject.transform.position = UserInputController.s_instance.GetWorldPoint();
		}

		private void WallBuildingRoutine()
		{
			if(_currentBetweenObject == null)
			{
				ConstructBetween();
			}

			float dis = 0;
			float length = 0;

			int x = 0;
			
			do
			{
				x++;

				float _angle = 0;
				Vector3 dir = (UserInputController.s_instance.GetWorldPoint() - _lastPole);
				dis = dir.magnitude;

				//if(dis < 1)
				//{
				//	return;
				//}
				length = _ghostWallBetween.GetComponent<BoxCollider>().size.z;

				// Wall Corner
				Vector3 v3Pos = Camera.main.WorldToScreenPoint(_lastPole);
				v3Pos = Input.mousePosition - v3Pos;
				_angle = Mathf.Atan2(v3Pos.y, v3Pos.x) * Mathf.Rad2Deg;
				v3Pos = Quaternion.AngleAxis(-_angle, Vector3.up) * (Camera.main.transform.right * (length + 0.5f));
				_currentPlaceableObject.transform.position = _lastPole + v3Pos;

				// Wall Between
				Vector3 dir2 = (_currentPlaceableObject.transform.position - _lastPole);
				Vector3 pos = dir2 * 0.5f + _lastPole;
				Quaternion rotationObj = Quaternion.LookRotation(dir2, Vector3.up);
				_currentBetweenObject.transform.position = pos;
				_currentBetweenObject.transform.rotation = rotationObj;


				if(dis > (length + 1f))
				{
					ConstructBetween();
					ConstructWall();
				}

			} while(dis > (length + 1f) && x < 10);
		}

		private void ConstructBetween()
		{
			Vector3 dir2 = (_currentPlaceableObject.transform.position - _lastPole);
			Vector3 pos = dir2 * 0.5f + _lastPole;
			Quaternion rotationObj = Quaternion.LookRotation(-dir2, Vector3.up);

			GameObject Obj = Instantiate(_ghostWallBetween, pos, rotationObj);
			_currentBetweenObject = Obj;
			_placedBuiltings.Add(Obj, ObjectType.WALL_BETWEEN);
		}

		private void ConstructWall()
		{
			if(_currentPlaceableObject == null)
			{
				_lastPole = UserInputController.s_instance.GetWorldPoint();
			}
			else
			{
				_lastPole = _currentPlaceableObject.transform.position;
			}

			_isBuilt = true;
			_currentPlaceableObject = null;
			_currentPlaceableObject = Instantiate(_ghostWall, UserInputController.s_instance.GetWorldPoint(), Quaternion.identity);
			_currentPrefabIndex++;
			_placedBuiltings.Add(_currentPlaceableObject, ObjectType.WALL);
		}




		private void PlaceRefineryPrefab()
		{
			//GameObject obj = Instantiate(_currentPlaceableObject, _currentPlaceableObject.transform.position, _currentPlaceableObject.transform.rotation);
			//RefineryRefHolder holder = obj.GetComponent<RefineryRefHolder>();
			//holder.RefineryPrefab.SetActive(false);
			//holder._Positions = UserInputController.s_instance.GetTexturePixelPoint();
			//holder.GetComponent<BuildingProcess>().Startbuilding();
			_currentPlaceableObject.GetComponent<TickBuildEmitter>().AddToGatherValue();
			_isBuilt = false;
			EndBuilding();
		}
	}
}
