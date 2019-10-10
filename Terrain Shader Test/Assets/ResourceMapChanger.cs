using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceMapChanger : MonoBehaviour
{
    protected static ResourceMapChanger s_Instance;
    public static ResourceMapChanger instance { get { return s_Instance; } }

    public Renderer texturRenderer;
    private Texture resourceTexture;
    private RenderTexture result;
    private int resourceCalcKernel;
    private bool changeMap = false;
    public List<Vector4> fabricCenter;

    public ComputeShader computeShader;
    public bool hierKoennteIhrTickStehen;

    public Vector3 ground;

    [SerializeField]
    private float PixelPerUnit = 1;

    [Tooltip("muss durch ppu teilbar sein!")]
    [SerializeField]
    private float mapWith;

    [Tooltip("muss durch ppu teilbar sein!")]
    [SerializeField]
    private float mapHeigth;

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
  //      texturRenderer = GetComponent<Renderer>();
        resourceTexture = Instantiate(texturRenderer.material.GetTexture("_NoiseMap")) as Texture2D;

        resourceCalcKernel = computeShader.FindKernel("CSMain");
        result = new RenderTexture(Mathf.RoundToInt(mapHeigth * PixelPerUnit), Mathf.RoundToInt(mapWith * PixelPerUnit), 24)
        {
            enableRandomWrite = true
        };
        result.Create();
    }

    private void Update()
    {
        if (fabricCenter.Count > 0 && hierKoennteIhrTickStehen)
        {
            CalcRes();
        }
    }

    public void AddFabric(Vector3 pos, float radius)
    {
        // (pos - texture position) * ppu
        Vector3 position = (pos - ground) * PixelPerUnit;
        print(pos);
        fabricCenter.Add(new Vector4(position.x, position.y, position.z, radius));
    }

    private void CalcRes()
    {
        computeShader.SetInt("PointSize", fabricCenter.Count);
        computeShader.SetVectorArray("coords", fabricCenter.ToArray());
        computeShader.SetTexture(resourceCalcKernel, "Result", result);

        computeShader.Dispatch(resourceCalcKernel, 512 / 8, 512 / 8, 1);

        texturRenderer.material.SetTexture("_NoiseMap", result);
    }

    public void SwitchMap()
    {
        changeMap = !changeMap;

        int t = changeMap ? 0 : 1;
        texturRenderer.material.SetFloat("_MetalResourcesInt", t);
    }
}
