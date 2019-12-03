using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PPBA
{
	public class TerritoriumMapCalculate : MonoBehaviour
	{
		public ComputeShader _TerritoriumComputeShader;
		public Renderer _GroundRenderer;
		public RenderTexture _currentTerritoriumMap;

		private RenderTexture _ResultTexture;
		private ComputeBuffer _buffer;

		private int _resourceCalcKernel;
		private int _resourceCalcKernel2;
		private List<RefineryRefHolder> _Refinerys = new List<RefineryRefHolder>();


		[SerializeField]
		private int[] _ResourceValues;

		void Start()
		{
			Texture2D resourceTexture = Instantiate(_GroundRenderer.material.GetTexture("_NoiseMap")) as Texture2D;
		
			_resourceCalcKernel = _TerritoriumComputeShader.FindKernel("CSMain");
			_resourceCalcKernel2 = _TerritoriumComputeShader.FindKernel("CSInit");

			_currentTerritoriumMap = new RenderTexture(resourceTexture.width, resourceTexture.height, 0, RenderTextureFormat.R8)
			{
				enableRandomWrite = true
			};

			_ResultTexture = new RenderTexture(512, 512, 0, RenderTextureFormat.R8)
			{
				enableRandomWrite = true
			};

			_ResultTexture.Create();
			Graphics.Blit(resourceTexture, _ResultTexture);
			Graphics.Blit(resourceTexture, _currentTerritoriumMap);

		}

		public void AddFabric(RefineryRefHolder refHolder)
		{

			_Refinerys.Add(refHolder);
		}

		public void RefreshCalcRes()
		{

			_ResourceValues = new int[_Refinerys.Count];
			_buffer = new ComputeBuffer(_Refinerys.Count, sizeof(int));

			_TerritoriumComputeShader.SetInt("PointSize", _Refinerys.Count);
			_TerritoriumComputeShader.SetVectorArray("coords", RefineriesProperties());

			_TerritoriumComputeShader.SetBuffer(_resourceCalcKernel2, "buffer", _buffer);
			_TerritoriumComputeShader.SetBuffer(_resourceCalcKernel, "buffer", _buffer);

			_TerritoriumComputeShader.SetTexture(_resourceCalcKernel, "InputTexture", _currentTerritoriumMap);
			_TerritoriumComputeShader.SetTexture(_resourceCalcKernel, "Result", _ResultTexture);

			_TerritoriumComputeShader.Dispatch(_resourceCalcKernel2, 50, 1, 1); // prüfen ob er hier wartet
			_TerritoriumComputeShader.Dispatch(_resourceCalcKernel, 512 / 8, 512 / 8, 1);

			_buffer.GetData(_ResourceValues);
			_buffer.Release();
			_buffer = null;

			Graphics.Blit(_ResultTexture, _currentTerritoriumMap);

			_GroundRenderer.material.SetTexture("_NoiseMap", _ResultTexture);
			SendToTickManager();

		}

		private void SendToTickManager()
		{
			//	ressourceManager.AddRessourcesToRefineries(CurrentValue);
		}

		private bool _changeMap = false;

		public void SwitchMap()
		{
			_changeMap = !_changeMap;

			int t = _changeMap ? 0 : 1;
			_GroundRenderer.material.SetFloat("_MetalResourcesInt", t);
		}


		private Vector4[] RefineriesProperties()
		{
			Vector4[] refsProp = new Vector4[_Refinerys.Count];

			for(int i = 0; i < _Refinerys.Count; i++)
			{
				refsProp[i] = _Refinerys[i].GetShaderProperties();
			}
			return refsProp;
		}
	}
}

