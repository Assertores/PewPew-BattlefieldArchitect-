﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSetPixels : MonoBehaviour
{
    int kernel;
    Renderer rend;
    public Texture2D texture;
    Material mat;
    bool schalter = false;
    public Color testcolor;
    public ComputeShader compute;
    public RenderTexture result;

    public Vector4[] center;
    public int size = 2;

    private void Awake()
    {
        //Renderer rend = GetComponent<Renderer>();
        //texture = Instantiate(rend.material.GetTexture("_NoiseMap")) as Texture2D;
        //rend.material.SetTexture("_NoiseMap", texture);

        rend = GetComponent<Renderer>();
        texture = Instantiate(rend.material.GetTexture("_NoiseMap")) as Texture2D;

    }
    void Start()
    {
        //kernel = compute.FindKernel("CSMain");
        //result = new RenderTexture(512, 512, 24);
        //result.enableRandomWrite = true;
        //result.Create();

        //compute.SetTexture(kernel, "Result", result);
        //compute.SetVector("coords", center);

        //compute.Dispatch(kernel, 512 / 8, 512 / 8, 1);

        //rend.material.SetTexture("_NoiseMap", result);

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

        compute.SetInt("PointSize", size);
        compute.SetVectorArray("coords", center);
        compute.SetTexture(kernel, "Result", result);

        compute.Dispatch(kernel, 512 / 8, 512 / 8, 1);

        rend.material.SetTexture("_NoiseMap", result);

        RaycastHit hit;
        if (Input.GetMouseButtonDown(0) && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 1000))
        {
            Renderer renderer = hit.transform.GetComponent<Renderer>();
            Texture2D texture = renderer.material.mainTexture as Texture2D;
            Vector2 pixelUV = hit.textureCoord;
            pixelUV.x = Mathf.FloorToInt(pixelUV.x *= texture.width);
            pixelUV.y = Mathf.FloorToInt(pixelUV.y *= texture.height);

            print("pixels " + pixelUV.x + " " + pixelUV.y);
        }
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