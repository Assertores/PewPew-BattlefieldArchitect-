using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class ResourceMapCalculate : Singleton<ResourceMapCalculate>
	{

		public ComputeShader _computeShader;
		public Renderer _GroundRenderer;

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
			_resourceCalcKernel = _computeShader.FindKernel("CSMain");
			_resourceCalcKernel2 = _computeShader.FindKernel("CSInit");


			_ResultTexture = new RenderTexture(512, 512, 24)
			{
				enableRandomWrite = true
			};

			_ResultTexture.Create();
			Graphics.Blit(resourceTexture, _ResultTexture);

		}

		public void AddFabric(RefineryRefHolder refHolder)
		{

			_Refinerys.Add(refHolder);
		}

		public void RefreshCalcRes()
		{
			_ResourceValues = new int[_Refinerys.Count];
			_buffer = new ComputeBuffer(_Refinerys.Count, sizeof(int));

			_computeShader.SetInt("PointSize", _Refinerys.Count);
			_computeShader.SetVectorArray("coords", RefineriesProperties());

			_computeShader.SetBuffer(_resourceCalcKernel2, "buffer", _buffer);
			_computeShader.SetBuffer(_resourceCalcKernel, "buffer", _buffer);

			_computeShader.SetTexture(_resourceCalcKernel, "Result", _ResultTexture);

			_computeShader.Dispatch(_resourceCalcKernel2, 50, 1, 1); // prüfen ob er hier wartet
			_computeShader.Dispatch(_resourceCalcKernel, 512 / 8, 512 / 8, 1);

			_buffer.GetData(_ResourceValues);
			_buffer.Release();
			_buffer = null;
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
			Vector4[] refsprop = new Vector4[_Refinerys.Count];

			for(int i = 0; i < _Refinerys.Count; i++)
			{
				refsprop[i] = _Refinerys[i].GetShaderProperties();
			}
			return refsprop;
		}
	}
		
}

