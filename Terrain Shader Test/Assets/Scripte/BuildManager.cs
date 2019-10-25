using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildManager : MonoBehaviour
{
	public LayerMask ignore;

	private MouseInputs inputs;

	private GameObject currentPlaceableObject;
	private GameObject currentBetweenObject;
	private InventoryItem curItem;

	private float mouseWheelRotation;
	private int currentPrefabIndex = 0;

	Vector2 pixelUV = Vector2.zero;

	public float radius = 100;
	public float intensity = 1;

	private bool creatingBuild = false;

	private GameObject lastPole;

	private void Start()
	{
		inputs = GetComponent<MouseInputs>();
	}

	private void Update()
	{

		if (currentPlaceableObject != null)
		{
			CancelBuilding();

			if (currentPrefabIndex == 0)
			{
				MoveCurrentObjectToMouse();
				RotateFromMouseWheel();

			}
			else if (curItem != null && curItem.canConnect)
			{
				CreateSegment();
			}

			ReleaseIfClicked();
		}
	}

	private void ReleaseIfClicked()
	{
		if (Input.GetMouseButtonDown(0))
		{
			if (!curItem.canConnect)
			{
				// to do in constructBuild einfügen
				ResourceMapChanger.instance.AddFabric(new Vector3(pixelUV.x, 0, pixelUV.y), intensity, radius);
				currentPlaceableObject = null;
				curItem = null;
				return;
			}
			ConstructBuild();
			currentPrefabIndex++;
			currentBetweenObject = Instantiate(curItem.ConnectingObject);

		}
	}

	public float angle = 0;
	private void CreateSegment()
	{
		Vector3 dir = (inputs.GetWorldPoint() - lastPole.transform.position);
		float dis = dir.magnitude;

		//if (dis < 1)
		//{
		//	return;
		//}
		float length = curItem.ConnectingObject.GetComponent<MeshRenderer>().bounds.size.z;

		Vector3 v3Pos = Camera.main.WorldToScreenPoint(lastPole.transform.position);
		v3Pos = Input.mousePosition - v3Pos;
		angle = Mathf.Atan2(v3Pos.y, v3Pos.x) * Mathf.Rad2Deg;
		v3Pos = Quaternion.AngleAxis(-angle, Vector3.up) * (Vector3.back * length);
		currentPlaceableObject.transform.position = lastPole.transform.position + v3Pos;


		Vector3 dir2 = (currentPlaceableObject.transform.position - lastPole.transform.position);
		Vector3 pos = dir2 * 0.5f  + lastPole.transform.position;
		Quaternion rotationObj = Quaternion.LookRotation(dir2, Vector3.up);
		currentBetweenObject.transform.position = pos;
		currentBetweenObject.transform.rotation = rotationObj;

	// todo länge des meshes einfügen
		if (dis > length)
		{
			ConstructBetween();
			ConstructBuild();
		}
	}

	private void ConstructBuild()
	{
		lastPole = currentPlaceableObject;
		currentPlaceableObject = null;
		currentPlaceableObject = Instantiate(curItem.prefab);
	}

	private void ConstructBetween()
	{
		Vector3 dir2 = (currentPlaceableObject.transform.position - lastPole.transform.position);
		Vector3 pos = dir2 * 0.5f + lastPole.transform.position;
		Quaternion rotationObj = Quaternion.LookRotation(dir2, Vector3.up);

		Instantiate(curItem.ConnectingObject, pos, rotationObj);

	}

	private void RotateFromMouseWheel()
    {
        mouseWheelRotation += Input.mouseScrollDelta.y;
        currentPlaceableObject.transform.Rotate(Vector3.up, mouseWheelRotation * 10);
    }

    private void MoveCurrentObjectToMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, 1000, ignore))
        {
            pixelUV = hitInfo.textureCoord;
            pixelUV.x = Mathf.FloorToInt(pixelUV.x *= hitInfo.transform.GetComponent<Renderer>().material.GetTexture("_NoiseMap").width);
            pixelUV.y = Mathf.FloorToInt(pixelUV.y *= hitInfo.transform.GetComponent<Renderer>().material.GetTexture("_NoiseMap").height);

            currentPlaceableObject.transform.position = hitInfo.point;
            currentPlaceableObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
        }
    }

    public void HandleNewObject(InventoryItem ip)
    {
        curItem = ip;
        currentPlaceableObject = Instantiate(curItem.prefab);
    }

    public void CancelBuilding()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
        {
            curItem = null;
            Destroy(currentPlaceableObject);
			currentPrefabIndex = 0;

		}
    }

    // old keyspress 1-9 for building
    //private void HandleNewObjectHotKey()
    //{
    //    for (int i = 0; i < placeObjectPrefabs.Length; i++)
    //    {
    //        if (Input.GetKeyDown(KeyCode.Alpha0 + 1 + i))
    //        {
    //            if (PressedKeyCurrentPrefab(i))
    //            {
    //                Destroy(currentPlaceableObject);
    //                currentPrefabIndex = -1;
    //            }
    //            else
    //            {
    //                if (currentPlaceableObject != null)
    //                {
    //                    Destroy(currentPlaceableObject);
    //                }
    //                currentPlaceableObject = Instantiate(placeObjectPrefabs[i]);
    //                currentPrefabIndex = i;
    //            }
    //            // not more then one buttonpress at frame
    //            break;

    //        }
    //    }

    //}

    //private bool PressedKeyCurrentPrefab(int i)
    //{
    //    return currentPlaceableObject != null && currentPrefabIndex == i;
    //}
}

