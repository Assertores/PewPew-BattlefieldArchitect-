using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HarvestResources : MonoBehaviour
{
    Texture2D texture;
    public TestSetPixels test;
    public bool isreduce = false;
    public float radius = 30;
    Vector2 pos;

    // Start is called before the first frame update
    void Start()
    {
        //pos = GetPixelUVPosition();
        //StartCoroutine(HarvestResource());
    }

    public void SetBuilding()
    {


    }


    //IEnumerator HarvestResource()
    //{
    //    while (true)
    //    {
    //        yield return new WaitForSeconds(0.2f);
    //        test.ChangeColor(texture, (int)pos.x, (int)pos.y, 20, isreduce);
    //    }
    //}

    //IEnumerator RefreshResouceValue()
    //{
    //    yield return new WaitForSeconds(1f);
    //    Vector2 pos = GetPixelUVPosition();
    //    float res = test.ReadRessourceValue(texture, (int)pos.x, (int)pos.y, 20);
    //}

    //Vector2 GetPixelUVPosition()
    //{
    //    RaycastHit hit;
    //    Vector2 pixelUV;

    //    if (Physics.Raycast(this.transform.position, Vector3.down, out hit, 2))
    //    {
    //        Renderer renderer = hit.transform.GetComponent<Renderer>();
    //        //Texture texture = renderer.material.mainTexture as Texture2D;
    //        texture = renderer.material.GetTexture("_NoiseMap") as Texture2D;

    //        pixelUV = hit.textureCoord;
    //        pixelUV.x = Mathf.FloorToInt(pixelUV.x *= texture.width);
    //        pixelUV.y = Mathf.FloorToInt(pixelUV.y *= texture.height);

    //        print("pixels " + pixelUV.x + " " + pixelUV.y);
    //        return pixelUV;
    //    }
    //    return Vector2.zero;
    //}

}
