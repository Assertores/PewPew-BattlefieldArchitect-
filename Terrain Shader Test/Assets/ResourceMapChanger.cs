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
    private int resourceCalcKernel2;
    private bool changeMap = false;
    public List<Vector4> fabricCenter = new List<Vector4>();

    public ComputeShader computeShader;
    ComputeBuffer buffer;

    public bool hierKoennteIhrTickStehen;

    public Vector3 ground;

    [SerializeField]
    private int[] values;

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
        //fabricCenter = new List<Vector4>();
        resourceTexture = Instantiate(texturRenderer.material.GetTexture("_NoiseMap")) as Texture2D;
        values = new int[50];
        resourceCalcKernel = computeShader.FindKernel("CSMain");
        resourceCalcKernel2 = computeShader.FindKernel("CSInit");

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
        }
    }

    public void AddFabric(Vector3 pos, float intensity, float radius)
    {
        fabricCenter.Add(new Vector4(pos.x, pos.z, intensity, radius));
    }

    private void RefreshCalcRes()
    {
        buffer = new ComputeBuffer(50, sizeof(int));

        computeShader.SetInt("PointSize", fabricCenter.Count);
        computeShader.SetVectorArray("coords", fabricCenter.ToArray());

        computeShader.SetBuffer(resourceCalcKernel2, "buffer", buffer);
        computeShader.SetBuffer(resourceCalcKernel, "buffer", buffer);

        computeShader.SetTexture(resourceCalcKernel, "Result", result);

        computeShader.Dispatch(resourceCalcKernel2, 50, 1, 1); // prüfen ob er hier wartet
        computeShader.Dispatch(resourceCalcKernel, 512 / 8, 512 / 8, 1);

        buffer.GetData(values);
        buffer.Release();
        buffer = null;
        texturRenderer.material.SetTexture("_NoiseMap", result);
    }

    public void SwitchMap()
    {
        changeMap = !changeMap;

        int t = changeMap ? 0 : 1;
        texturRenderer.material.SetFloat("_MetalResourcesInt", t);
    }
}
