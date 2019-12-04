using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class TerritoriumMapCalculate : Singleton<TerritoriumMapCalculate>
	{
		public ComputeShader _computeShader;
		public Material _GroundMaterial;

		private RenderTexture TerritorriumMap;
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

			Texture2D resourceTexture = Instantiate(_GroundMaterial.GetTexture("_TerritorriumMap")) as Texture2D;
			_resourceCalcKernel = _computeShader.FindKernel("CSTerritorium");

			_ResultTexture = new RenderTexture(resourceTexture.width, resourceTexture.height, 0, RenderTextureFormat.R8)
			{
				enableRandomWrite = true
			};

			_ResultTexture.Create();
			Graphics.Blit(resourceTexture, _ResultTexture);
			Graphics.Blit(resourceTexture, TerritorriumMap);

			_GroundMaterial.SetTexture("_TerritorriumMap", _ResultTexture);
		}



		public void RefreshCalcRes()
		{
			_computeShader.SetTexture(_resourceCalcKernel, "TerritoriumResult", _ResultTexture);

			_computeShader.Dispatch(_resourceCalcKernel, 512 / 8, 512 / 8, 1);

			_GroundMaterial.SetTexture("_TerritorriumMap", _ResultTexture);
			SendToTickManager();
		}

		private void SendToTickManager()
		{
			//	ressourceManager.AddRessourcesToRefineries(CurrentValue);
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

		void Update()
		{
			if(Input.GetKeyDown(KeyCode.M))
			{
				Vector4[] test = new Vector4[1];
				test[0] = new Vector4(100.0f, 100.0f, 1.0f);
				_computeShader.SetVectorArray("Soldiers", test);
				RefreshCalcRes();
			}
		}

		void OnDisable()
		{
			_GroundMaterial.SetTexture("_TerritorriumMap", TerritorriumMap);
		}
	}

}

