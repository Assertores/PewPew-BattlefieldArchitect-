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


    private void Update()
    {
        HandleNewObjectHotKey();
        
        if (currentPlaceableObject!= null)
        {
            MoveCurrentObjectToMouse();
            RotateFromMouseWheel();
            ReleaseIfClicked();
        }
    }

    private void ReleaseIfClicked()
    {
        if (Input.GetMouseButtonDown(0))
        {
            ResourceMapChanger.instance.AddFabric(new Vector3(pixelUV.x , 0 , pixelUV.y), intensity, radius);
            currentPlaceableObject = null;
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

    private void HandleNewObjectHotKey()
    {
        for (int i = 0; i < placeObjectPrefabs.Length; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + 1 + i))
            {
                if (PressedKeyCurrentPrefab(i))
                {
                    Destroy(currentPlaceableObject);
                    currentPrefabIndex = -1;
                }
                else
                {
                    if (currentPlaceableObject != null)
                    {
                        Destroy(currentPlaceableObject);
                    }
                    currentPlaceableObject = Instantiate(placeObjectPrefabs[i]);
                    currentPrefabIndex = i;
                }
                // not more then one buttonpress at frame
                break;

            }
        }

    }

    private bool PressedKeyCurrentPrefab(int i)
    {
        return currentPlaceableObject != null && currentPrefabIndex == i;
    }
}
