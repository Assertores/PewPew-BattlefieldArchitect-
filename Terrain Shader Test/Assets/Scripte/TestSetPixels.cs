using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSetPixels : MonoBehaviour
{
    public Texture2D texture;
    Material mat;
    bool schalter = false;
    public Color testcolor;


    private void Awake()
    {
        Renderer rend = GetComponent<Renderer>();
        texture = Instantiate(rend.material.GetTexture("_NoiseMap")) as Texture2D;
        rend.material.SetTexture("_NoiseMap", texture);

    }
    void Start()
    {

        //// colors used to tint the first 3 mip levels
        //Color[] colors = new Color[3];
        //colors[0] = Color.red;
        //colors[1] = Color.green;
        //colors[2] = Color.blue;
        //int mipCount = Mathf.Min(3, texture.mipmapCount);

        //// tint each mip level
        //for (int mip = 0; mip < mipCount; ++mip)
        //{
        //    Color[] cols = texture.GetPixels(mip);
        //    for (int i = 0; i < cols.Length; ++i)
        //    {
        //        cols[i] = Color.Lerp(cols[i], colors[mip], 0.33f);
        //    }
        //    texture.SetPixels(cols, mip);
        //}
        //// actually apply all SetPixels, don't recalculate mip levels
        //texture.Apply(false);
    }

    private void Update()
    {

        //RaycastHit hit;
        //if (Input.GetMouseButtonDown(0) && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 1000))
        //{
        //    Renderer renderer = hit.transform.GetComponent<Renderer>();
        //    Texture2D texture = renderer.material.mainTexture as Texture2D;
        //    Vector2 pixelUV = hit.textureCoord;
        //    pixelUV.x = Mathf.FloorToInt(pixelUV.x *= texture.width);
        //    pixelUV.y = Mathf.FloorToInt(pixelUV.y *= texture.height);

        //    print("pixels " + pixelUV.x + " " + pixelUV.y);
        //}
    }

    public void kreis()
    {
        //Circle(texture, texture.width / 2, texture.height / 2, 20,);

    }

    public float ReadRessourceValue(Texture2D tex, int cx, int cy, int r)
    {
        int x, y, px, nx, py, ny, d;

        float resources = 0;

        Color[] tempArray = tex.GetPixels();

        print("anzahl " + tempArray.Length);

        for (x = 0; x <= r; x++)
        {
            d = (int)Mathf.Ceil(Mathf.Sqrt(r * r - x * x));
            for (y = 0; y <= d; y++)
            {
                px = cx + x;
                nx = cx - x;
                py = cy + y;
                ny = cy - y;

                resources += tempArray[py * tex.width + px].grayscale;
                resources += tempArray[py * tex.width + nx].grayscale;
                resources += tempArray[ny * tex.width + px].grayscale;
                resources += tempArray[ny * tex.width + nx].grayscale;
            }
        }
        return resources;

    }

    public void ChangeColor(Texture2D tex, int cx, int cy, int r, bool isReduce)
    {
        int x, y, px, nx, py, ny, d;
        Color[] tempArray = tex.GetPixels();
        Color col;
        col = isReduce ? new Color(-0.01f, -0.01f, -0.01f) : new Color(0.01f, 0.01f, 0.01f);

        for (x = 0; x <= r; x++)
        {
            d = (int)Mathf.Ceil(Mathf.Sqrt(r * r - x * x));
            for (y = 0; y <= d; y++)
            {
                px = cx + x;
                nx = cx - x;
                py = cy + y;
                ny = cy - y;

                tempArray[py * tex.width + px] += col;
                tempArray[py * tex.width + nx] += col;
                tempArray[ny * tex.width + px] += col;
                tempArray[ny * tex.width + nx] += col;
            }
        }
        tex.SetPixels(tempArray);
        tex.Apply();
    }




    public void Circle(Texture2D tex, int cx, int cy, int r, Color col)
    {
        int x, y, px, nx, py, ny, d;
        Color[] tempArray = tex.GetPixels();

        for (x = 0; x <= r; x++)
        {
            d = (int)Mathf.Ceil(Mathf.Sqrt(r * r - x * x));
            for (y = 0; y <= d; y++)
            {
                px = cx + x;
                nx = cx - x;
                py = cy + y;
                ny = cy - y;

                tempArray[py * tex.width + px] = col;
                tempArray[py * tex.width + nx] = col;
                tempArray[ny * tex.width + px] = col;
                tempArray[ny * tex.width + nx] = col;

                //count += tempArray[py * tex.width + px].grayscale;
                //count += tempArray[py * tex.width + nx].grayscale;
                //count += tempArray[ny * tex.width + px].grayscale;
                //count += tempArray[ny * tex.width + nx].grayscale;

                //print(tempArray[ny * tex.width + nx].grayscale);
            }
        }
        tex.SetPixels(tempArray);
        tex.Apply();
    }

    public void SwitchMap()
    {
        schalter = !schalter;

        int t = schalter ? 0 : 1;
        mat.SetFloat("_MetalResourcesInt", t);
    }

}
