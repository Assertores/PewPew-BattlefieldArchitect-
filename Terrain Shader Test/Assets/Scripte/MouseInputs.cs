using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseInputs : MonoBehaviour
{
    public LayerMask ignore;
    
    public Vector3 GetWorldPoint()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, 1000, ignore))
        {
            return hitInfo.point;
        }
        return Vector3.zero;
    }

    public Vector2 GetTexturePixelPoint()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector2 pixelUV;

        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, 1000, ignore))
        {
            pixelUV = hitInfo.textureCoord;
            pixelUV.x = Mathf.FloorToInt(pixelUV.x *= hitInfo.transform.GetComponent<Renderer>().material.GetTexture("_NoiseMap").width);
            pixelUV.y = Mathf.FloorToInt(pixelUV.y *= hitInfo.transform.GetComponent<Renderer>().material.GetTexture("_NoiseMap").height);
            return pixelUV;
        }
        return Vector3.zero;
    }

    public Vector3 SnapPosition(Vector3 originalPos)
    {
        Vector3 snapped;
        snapped.x = Mathf.Floor(originalPos.x + 0.1f);
        snapped.y = Mathf.Floor(originalPos.y + 0.1f);
        snapped.z = Mathf.Floor(originalPos.z + 0.1f);
        return snapped;
    }
}
