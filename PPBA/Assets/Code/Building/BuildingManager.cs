using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class BuildingManager : Singleton<BuildingManager>
	{
		//public GameObject GhostRefinery;
		public GameObject GhostWall;
		public GameObject GhostWallBetween;

		private ObjectType _CurrentObjectType;
		private GameObject _currentPlaceableObject;
		private GameObject _CurrentBetweenObject;
		private Vector3 _lastPole;
		private bool _isBuilt = false;

		private Dictionary<GameObject, ObjectType> _PlayedBuilts = new Dictionary<GameObject, ObjectType>();
		public bool _canBuild = true;

		public float _MouseWheelRotation;
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
					if(!_canBuild)
					{   // for the last one with this we can go on building

						foreach(KeyValuePair<GameObject, ObjectType> build in _PlayedBuilts)
						{
							Destroy(build.Key);
						}
					}
					_currentPrefabIndex = 0;
					_isBuilt = false;

					foreach(KeyValuePair<GameObject, ObjectType> build in _PlayedBuilts)
					{
						build.Key.GetComponent<TickBuildEmitter>().AddToGatherValue();
						// TODO : als einzelne objecte übertragen und aufm server WallBEtween setzen
					}

					_PlayedBuilts.Clear();

				}
			}
		}

		private void ReleaseIfClicked()
		{
			if(_CurrentObjectType != ObjectType.WALL)
			{
				PlaceRefineryPrefab();
			}

			if(_CurrentObjectType == ObjectType.WALL)
			{
				ConstructWall();
				_CurrentBetweenObject = Instantiate(GhostWallBetween, UserInputController.s_instance.GetWorldPoint(), Quaternion.identity);
			}
		}

		public void HandleNewObject(IRefHolder PrefabBuildingType)
		{
			_CurrentObjectType = PrefabBuildingType._Type;

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
			_currentPrefabIndex = 0;
			_isBuilt = false;
			_PlayedBuilts.Clear();
			_canBuild = true;


		}

		public void CancelBuilding()
		{
			Destroy(_currentPlaceableObject);
			_currentPrefabIndex = 0;
			_isBuilt = false;
			_canBuild = true;

			foreach(KeyValuePair<GameObject, ObjectType> build in _PlayedBuilts)
			{
				Destroy(build.Key);
			}
			_PlayedBuilts.Clear();
		}

		private void RotateCurrentObjectWithMouseWheel()
		{
			if(Input.GetKey(KeyCode.R))
			{
				//_MouseWheelRotation = Input.mouseScrollDelta.y;
				_currentPlaceableObject.transform.Rotate(Vector3.up, _MouseWheelRotation * Time.deltaTime);
			}
			if(Input.GetKey(KeyCode.T))
			{
				//_MouseWheelRotation = Input.mouseScrollDelta.y;
				_currentPlaceableObject.transform.Rotate(Vector3.up, -_MouseWheelRotation * Time.deltaTime);
			}
		}

		private void MoveCurrentObjectToMouse()
		{
			_currentPlaceableObject.transform.position = UserInputController.s_instance.GetWorldPoint();
		}

		private void WallBuildingRoutine()
		{
			if(_CurrentBetweenObject == null)
			{
				ConstructBetween();
			}

			float _angle = 0;
			Vector3 dir = (UserInputController.s_instance.GetWorldPoint() - _lastPole);
			float dis = dir.magnitude;

			//if (dis < 1)
			//{
			//	return;
			//}
			float length = GhostWallBetween.GetComponent<BoxCollider>().size.z;

			Vector3 v3Pos = Camera.main.WorldToScreenPoint(_lastPole);
			v3Pos = Input.mousePosition - v3Pos;
			_angle = Mathf.Atan2(v3Pos.y, v3Pos.x) * Mathf.Rad2Deg;
			v3Pos = Quaternion.AngleAxis(-_angle, Vector3.up) * (Camera.main.transform.right * length);
			_currentPlaceableObject.transform.position = _lastPole + v3Pos;


			Vector3 dir2 = (_currentPlaceableObject.transform.position - _lastPole);
			Vector3 pos = dir2 * 0.5f + _lastPole;
			Quaternion rotationObj = Quaternion.LookRotation(dir2, Vector3.up);
			_CurrentBetweenObject.transform.position = pos;
			_CurrentBetweenObject.transform.rotation = rotationObj;

			// todo länge des meshes einfügen
			if(dis > length)
			{
				ConstructBetween();
				ConstructWall();
			}
		}

		private void ConstructBetween()
		{
			Vector3 dir2 = (_currentPlaceableObject.transform.position - _lastPole);
			Vector3 pos = dir2 * 0.5f + _lastPole;
			Quaternion rotationObj = Quaternion.LookRotation(dir2, Vector3.up);

			GameObject Obj = Instantiate(GhostWallBetween, pos, rotationObj);
			_PlayedBuilts.Add(Obj, ObjectType.WALL_BETWEEN);
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
			_currentPlaceableObject = Instantiate(GhostWall, UserInputController.s_instance.GetWorldPoint(), Quaternion.identity);
			_currentPrefabIndex++;
			_PlayedBuilts.Add(_currentPlaceableObject, ObjectType.WALL);
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
