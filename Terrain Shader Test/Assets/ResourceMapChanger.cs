using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceMapChanger : MonoBehaviour
{
    protected static ResourceMapChanger s_Instance;
    public static ResourceMapChanger instance { get { return s_Instance; } }

    public Renderer texturRenderer;
    private Texture2D resourceTexture;
    private RenderTexture result;
    private int resourceCalcKernel;
    private bool changeMap = false;
    public List<Vector4> fabricCenter;

    public ComputeShader computeShader;
    ComputeBuffer buffer;
        
    public bool hierKoennteIhrTickStehen;

    public Vector3 ground;

    public float[] values;

    private void Awake()
    {
        if (s_Instance == null)
        {
            s_Instance = this;
        }
        else
        {
            Debug.LogWarning("Double ResourceMapChanger in Scene!!");
        }
    }

    private void Start()
    {
        fabricCenter = new List<Vector4>();
        resourceTexture = Instantiate(texturRenderer.material.GetTexture("_NoiseMap")) as Texture2D;
        //texturRenderer.material.SetTexture("_NoiseMap", result);

        //resourceTexture.Resize(Mathf.RoundToInt(mapHeigth * PixelPerUnit), Mathf.RoundToInt(mapWith * PixelPerUnit));
        //resourceTexture.Apply();

        //result = Instantiate(texturRenderer.material.GetTexture("_NoiseMap")) as RenderTexture;

        buffer = new ComputeBuffer(50, sizeof(float));
        values = new float[50];

        resourceCalcKernel = computeShader.FindKernel("CSMain");
        //result = new RenderTexture(Mathf.RoundToInt(mapHeigth * PixelPerUnit), Mathf.RoundToInt(mapWith * PixelPerUnit), 24, RenderTextureFormat.RFloat)
        //{
        //    enableRandomWrite = true
        //};

        result = new RenderTexture(resourceTexture.height, resourceTexture.width, 24)
        {
            enableRandomWrite = true

        };

        result.Create();
        Graphics.CopyTexture(resourceTexture, result);
    }


    private void Update()
    {
        if (isRunning == false && fabricCenter.Count > 0)
        {
            StartCoroutine(StartResourceHarvest());
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            RefreshCalcRes();
        }


    }

    bool isRunning = false;

    IEnumerator StartResourceHarvest()
    {
        isRunning = true;
        while (fabricCenter.Count > 0)
        {
            yield return new WaitForSeconds(0.1f);
            //RefreshCalcRes();
        }
    }

    public void AddFabric(Vector3 pos, float intensity, float radius)
    {
        // (pos - texture position) * ppu
        //  Vector3 position = (pos - ground) * PixelPerUnit;

        fabricCenter.Add(new Vector4(pos.x, pos.z, intensity, radius));
        //NewhCalcBuild(new Vector4(pos.x, pos.z, intensity, radius));
    }

    //public void NewCalcBuild(Vector4 build)
    //{
    //    Vector4[] _builds = new Vector4[1];
    //    _builds[0] = build;

    //    computeShader.SetTexture(resourceCalcKernel, "Result", result);
    //    computeShader.SetInt("PointSize", _builds.Length);
    //    computeShader.SetVectorArray("coords", _builds);
    //    computeShader.SetTexture(resourceCalcKernel, "Result", result);

    //    computeShader.Dispatch(resourceCalcKernel, 512 / 8, 512 / 8, 1);

    //    texturRenderer.material.SetTexture("_NoiseMap", result);
    //}


    private void RefreshCalcRes()
    {
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = 0;
        }

        //computeShader.SetTexture(resourceCalcKernel, "Result", result);
        computeShader.SetInt("PointSize", fabricCenter.Count);
        computeShader.SetVectorArray("coords", fabricCenter.ToArray());
        computeShader.SetBuffer(resourceCalcKernel, "buffer", buffer);

        computeShader.SetTexture(resourceCalcKernel, "Result", result);

        computeShader.Dispatch(resourceCalcKernel, 512 / 8, 512 / 8, 1);

        buffer.GetData(values);
        texturRenderer.material.SetTexture("_NoiseMap", result);

        for (int i = 0; i < fabricCenter.Count; i++)
        {
            print("value " + values[i] + "counter " + i);

        }
    }

    public void SwitchMap()
    {
        changeMap = !changeMap;

        int t = changeMap ? 0 : 1;
        texturRenderer.material.SetFloat("_MetalResourcesInt", t);
    }
}
