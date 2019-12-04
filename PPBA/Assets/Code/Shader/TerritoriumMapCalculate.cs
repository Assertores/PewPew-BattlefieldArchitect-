using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class TerritoriumMapCalculate : Singleton<TerritoriumMapCalculate>
	{
		public ComputeShader _computeShader;
		public Material _GroundMaterial;

		public Texture TerritorriumMap;
		private RenderTexture _ResultTexture;
		private ComputeBuffer _buffer;

		private int _resourceCalcKernel;
		private int _resourceCalcKernel2;
		private List<RefineryRefHolder> _Refinerys = new List<RefineryRefHolder>();

		private List<Vector4> _Soldiers = new List<Vector4>();


		void Start()
		{
			if(_GroundMaterial == null)
			{
				Debug.LogError("No Ground Material Found!!! (ResourceMapCalculate)");
				return;
			}

			Texture2D resourceTexture = Instantiate(_GroundMaterial.GetTexture("_TerritorriumMap")) as Texture2D;
			_resourceCalcKernel = _computeShader.FindKernel("CSTerritorium");

			_ResultTexture = new RenderTexture(resourceTexture.width, resourceTexture.height, 0, RenderTextureFormat.ARGB32)
			{
				enableRandomWrite = true
			};

			_ResultTexture.Create();
			Graphics.Blit(resourceTexture, _ResultTexture);

			_GroundMaterial.SetTexture("_TerritorriumMap", _ResultTexture);
		}

		public void RefreshCalcTerritorium()
		{
			_computeShader.SetInt("SoldiersSize", _Soldiers.Count);
			_computeShader.SetTexture(_resourceCalcKernel, "TerritoriumResult", _ResultTexture);
			_computeShader.SetVectorArray("coords", AddSoldier());

			_computeShader.Dispatch(_resourceCalcKernel, 512 / 8, 512 / 8, 1);

			_GroundMaterial.SetTexture("_TerritorriumMap", _ResultTexture);
			SendToTickManager();
		}

		private void SendToTickManager()
		{
			//	ressourceManager.AddRessourcesToRefineries(CurrentValue);
		}

		private Vector4[] AddSoldier()
		{
			Vector4[] refsProp = new Vector4[_Soldiers.Count];

			for(int i = 0; i < _Soldiers.Count; i++)
			{
				refsProp[i] = new Vector4(_Soldiers[i].x , _Soldiers[i].y, _Soldiers[i].z, 0);
			}
			return refsProp;
		}


		public int AddSoldier(Transform sol, Vector2 pos, int Team)
		{

			_Soldiers.Add(new Vector4(pos.x , pos.y, Team ));
			return _Soldiers.Count-1;
		}


		//void Update()
		//{
		//	if(Input.GetKeyDown(KeyCode.M))
		//	{
		//		Vector4[] test = new Vector4[2];
		//		test[0] = new Vector4(100.0f, 100.0f, 1.0f, 0);
		//		test[1] = new Vector4(256.0f, 256.0f, 0.0f, 0);
		//		_computeShader.SetVectorArray("Soldiers", test);
		//		RefreshCalcTerritorium();
		//	}
		//}

		void OnDisable()
		{
			_GroundMaterial.SetTexture("_TerritorriumMap", TerritorriumMap);
		}
	}

}

