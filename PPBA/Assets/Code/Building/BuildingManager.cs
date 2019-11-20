using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class BuildingManager : Singleton<BuildingManager>
	{
		public GameObject GhostRefinery;
		public GameObject GhostWall;

		private ObjectType _CurrentObjectType;
		private GameObject _currentPlaceableObject;
		private Vector3 _lastPole;
		private bool _isBuilt = false;
		private List<GameObject> _PlacedBuilt = new List<GameObject>();

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

				if(!_isBuilt)
				{
					MoveCurrentObjectToMouse();
					RotateCurrentObjectWithMouseWheel();

					if(Input.GetMouseButtonDown(0))
					{
						if(_canBuild)
						{
							ReleaseIfClicked();
						}
					}
				}

				if(Input.GetMouseButton(0) && _currentPrefabIndex > 0)
				{
					CreateSegment();
				}

				if(Input.GetMouseButtonUp(0) && _isBuilt)
				{
					if(!_canBuild)
					{	// for the last one with this we can go on building
						for(int i = 0; i < _PlacedBuilt.Count; i++)
						{
							if(i == _PlacedBuilt.Count-1)
							{
								continue;
							}
							Destroy(_PlacedBuilt[i]);
						}
					}
					_currentPrefabIndex = 0;
					_isBuilt = false;
					_PlacedBuilt.Clear();

				}
			}
		}

		private void ReleaseIfClicked()
		{
			if(_CurrentObjectType == ObjectType.REFINERY /*|| anderes single object*/)
			{
				PlaceRefineryPrefab();
			}

			if(_CurrentObjectType == ObjectType.WALL)
			{
				ConstructWall();
			}
		}

		public void HandleNewObject(ObjectType PrefabBuildingType)
		{
			_CurrentObjectType = PrefabBuildingType;

			if(PrefabBuildingType == ObjectType.REFINERY)
			{
				_currentPlaceableObject = Instantiate(GhostRefinery, UserInputController.s_instance.GetWorldPoint(), Quaternion.identity);
			}

			if(PrefabBuildingType == ObjectType.WALL)
			{
				ConstructWall();
			}
		}

		public void EndBuilding()
		{
			_currentPlaceableObject = null;
			_currentPrefabIndex = 0;
			_isBuilt = false;
			_PlacedBuilt.Clear();

		}

		public void CancelBuilding()
		{
			Destroy(_currentPlaceableObject);
			_currentPrefabIndex = 0;
			_isBuilt = false;
			_canBuild = true;

			for(int i = 0; i < _PlacedBuilt.Count; i++)
			{
				Destroy(_PlacedBuilt[i]);
			}
			_PlacedBuilt.Clear();
		}

		private void RotateCurrentObjectWithMouseWheel()
		{
			if(Input.GetKey(KeyCode.R))
			{
				//_MouseWheelRotation = Input.mouseScrollDelta.y;
				_currentPlaceableObject.transform.Rotate(Vector3.up, _MouseWheelRotation * Time.deltaTime );
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

		private void CreateSegment()
		{
			float _angle = 0;
			float length = _currentPlaceableObject.GetComponent<BoxCollider>().size.z;
			Vector3 lastPos = _lastPole;

			Vector3 dir = (UserInputController.s_instance.GetWorldPoint() - lastPos);
			float dis = dir.magnitude;

			Vector3 forward = _lastPole;

			float dotProd = Vector3.Dot(forward, dir);
			//print("dot " + dotProd);

			if(dotProd < 0)
			{
				lastPos = new Vector3(lastPos.x + (length * 0.5f), lastPos.y, lastPos.z);
			}
			else
			{
				lastPos = new Vector3(lastPos.x - (length * 0.5f), lastPos.y, lastPos.z);
			}

			//if(dis < 1)
			//{
			//	return;
			//}

			Vector3 v3Pos = Camera.main.WorldToScreenPoint(lastPos);
			v3Pos = Input.mousePosition - v3Pos;
			_angle = Mathf.Atan2(v3Pos.y, v3Pos.x) * Mathf.Rad2Deg;
			v3Pos = Quaternion.AngleAxis(-_angle, Vector3.up) * (Camera.main.transform.right * length);

			Vector3 dir2 = (_currentPlaceableObject.transform.position - lastPos).normalized;
			Quaternion rotationObj = Quaternion.LookRotation(dir2, Vector3.up);
			_currentPlaceableObject.transform.rotation = rotationObj;

			_currentPlaceableObject.transform.position = lastPos + v3Pos;

			if(dis > length)
			{
				ConstructWall();
			}
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
			_PlacedBuilt.Add(_currentPlaceableObject);
		}


		private void PlaceRefineryPrefab()
		{
			//GameObject obj = Instantiate(_currentPlaceableObject, _currentPlaceableObject.transform.position, _currentPlaceableObject.transform.rotation);
			//RefineryRefHolder holder = obj.GetComponent<RefineryRefHolder>();
			//holder.RefineryPrefab.SetActive(false);
			//holder._Positions = UserInputController.s_instance.GetTexturePixelPoint();
			//holder.GetComponent<BuildingProcess>().Startbuilding();

			_isBuilt = false;
			EndBuilding();
		}
	}
}
