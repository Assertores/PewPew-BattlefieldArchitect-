using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildManager : MonoBehaviour
{
    public LayerMask ignore;

    [SerializeField]
    private GameObject[] placeObjectPrefabs;

    private GameObject currentPlaceableObject;

    private float mouseWheelRotation;
    private int currentPrefabIndex = -1;

    Vector2 pixelUV = Vector2.zero;

    public float radius = 100;
    public float intensity = 1;

    private bool isBuildingActiv = false;
    private InventoryItem curItem;

    private void Update()
    {
       
        if (currentPlaceableObject!= null)
        {
            isCancelBuilding();
            MoveCurrentObjectToMouse();
            RotateFromMouseWheel();
            ReleaseIfClicked();
        }
    }

    int _buildingINdex;

    private void ReleaseIfClicked()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!curItem.canConnect)
            {
                ResourceMapChanger.instance.AddFabric(new Vector3(pixelUV.x , 0 , pixelUV.y), intensity, radius);
                currentPlaceableObject = null;
            }
            else
            {
                _buildingINdex++;
                currentPlaceableObject = null;
                currentPlaceableObject = Instantiate(curItem.prefab);
                ConnectingBuildings();
            }
        }
    }

    private void ConnectingBuildings()
    {
        if (_buildingINdex < 0)
        {

        }
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
        if (Physics.Raycast(ray, out hitInfo,1000, ignore))
        {
            pixelUV = hitInfo.textureCoord;
            pixelUV.x = Mathf.FloorToInt(pixelUV.x *= hitInfo.transform.GetComponent<Renderer>().material.GetTexture("_NoiseMap").width);
            pixelUV.y = Mathf.FloorToInt(pixelUV.y *= hitInfo.transform.GetComponent<Renderer>().material.GetTexture("_NoiseMap").height);

            currentPlaceableObject.transform.position = hitInfo.point;
            currentPlaceableObject.transform.rotation = Quaternion.FromToRotation(Vector3.up,hitInfo.normal);
        }
    }

    public void HandleNewObject(InventoryItem ip)
    {
        curItem = ip;
        currentPlaceableObject = Instantiate(curItem.prefab);
    }

    public void isCancelBuilding()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
        {
            curItem = null;
            Destroy(currentPlaceableObject);
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

