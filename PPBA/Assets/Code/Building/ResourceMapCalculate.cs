using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class ResourceMapCalculate : Singleton<ResourceMapCalculate>
	{

		public ComputeShader _computeShader;
		public Material _GroundMaterial;
		public RenderTexture _currentTexture;

		private RenderTexture _ResultTexture;
		private ComputeBuffer _buffer;

		private int _resourceCalcKernel;
		private int _resourceCalcKernel2;
		private List<RefineryRefHolder> _Refinerys = new List<RefineryRefHolder>();


		[SerializeField]
		private int[] _ResourceValues;
		
		void Start()
		{
			if(_GroundMaterial == null)
			{
				Debug.LogError("No Ground Material Found!!! (ResourceMapCalculate)");
				return;
			}

			Texture2D resourceTexture = Instantiate(_GroundMaterial.GetTexture("_NoiseMap")) as Texture2D;
			_resourceCalcKernel = _computeShader.FindKernel("CSMain");
			_resourceCalcKernel2 = _computeShader.FindKernel("CSInit");

			_currentTexture = new RenderTexture(resourceTexture.width, resourceTexture.height, 0, RenderTextureFormat.R8)
			{
				enableRandomWrite = true
			};

			_ResultTexture = new RenderTexture(resourceTexture.width, resourceTexture.height, 0, RenderTextureFormat.R8)
			{
				enableRandomWrite = true
			};

			_ResultTexture.Create();
			Graphics.Blit(resourceTexture, _ResultTexture);
			Graphics.Blit(resourceTexture, _currentTexture);

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

			_computeShader.SetTexture(_resourceCalcKernel, "InputTexture", _currentTexture);
			_computeShader.SetTexture(_resourceCalcKernel, "Result", _ResultTexture);

			_computeShader.Dispatch(_resourceCalcKernel2, 50, 1, 1); // prüfen ob er hier wartet
			_computeShader.Dispatch(_resourceCalcKernel, 512 / 8, 512 / 8, 1);

			_buffer.GetData(_ResourceValues);
			_buffer.Release();
			_buffer = null;

			Graphics.Blit(_ResultTexture, _currentTexture);

			_GroundMaterial.SetTexture("_NoiseMap", _ResultTexture);
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
			_GroundMaterial.SetFloat("_MapChange", t);
		}

		public void SwitchMapTerrritorrium()
		{
			_changeMap = !_changeMap;

			int t = _changeMap ? 0 : 2;
			_GroundMaterial.SetFloat("_MapChange", t);
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

