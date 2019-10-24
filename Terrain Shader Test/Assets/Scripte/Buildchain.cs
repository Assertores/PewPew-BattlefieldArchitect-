using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buildchain : MonoBehaviour
{
    public LayerMask ignore;

    InventoryItem currentBuilding;
    private GameObject currentPlaceableObject;
    private MouseInputs inputs;

    private void Start()
    {
        inputs = GetComponent<MouseInputs>();
    }

    // Update is called once per frame
    void Update()
    {

        if (currentBuilding != null)
        {
            MoveCurrentObjectToMouse();

        }
    }


    private void MoveCurrentObjectToMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, 1000, ignore))
        {
            currentPlaceableObject.transform.position = hitInfo.point;
            currentPlaceableObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
        }
    }

    public void HandleNewObject(InventoryItem ip)
    {
        currentBuilding = ip;
        currentPlaceableObject = Instantiate(currentBuilding.prefab);
    }
}
