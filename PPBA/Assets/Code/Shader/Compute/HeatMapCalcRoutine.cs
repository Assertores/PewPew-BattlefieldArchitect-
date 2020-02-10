using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PPBA
{
	public class HeatMapCalcRoutine : Singleton<HeatMapCalcRoutine>
	{
		SetTextureMapCalculate _setTexCalc;
		ResourceMapCalculate _resMapCalc;
		TerritoriumMapCalculate _terMapCalc;
		//EarlyCalculate _earlyCalc;

		[SerializeField] ComputeShader _computeShader;
		public Material _GroundMaterial;
		public Material _FogMaterial;

		public RenderTexture _ResultTextureRessource;
		public RenderTexture _ResultTextureTerritorium;
		public RenderTexture _ResultMyTextureTerritorium;
		//public RenderTexture _Resulttest;
		//public RenderTexture _ResultTerritoriumtest;

		public List<IRefHolder> _Refinerys = new List<IRefHolder>();
		public Dictionary<Transform, int> _Soldiers = new Dictionary<Transform, int>();

		public Texture2D startResMap;
		public Texture2D startTerMap;

		private void OnEnable()
		{
			startResMap = _GroundMaterial.GetTexture("_NoiseMap") as Texture2D;
			startTerMap = _GroundMaterial.GetTexture("_TerritorriumMap") as Texture2D;
		}

		private void OnDisable()
		{
			_GroundMaterial.SetTexture("_NoiseMap", startResMap);
			_GroundMaterial.SetTexture("_TerritorriumMap", startTerMap);
		}

		private void Start()
		{

			_setTexCalc = new SetTextureMapCalculate(_computeShader.FindKernel("CSBitToTex"), GlobalVariables.s_instance._teamColors);
			_setTexCalc._computeShader = _computeShader;

			//gameObject.AddComponent<SetTextureMapCalculate>();
			//_setTexCalc = GetComponent<SetTextureMapCalculate>();
			//_setTexCalc._computeShader = _computeShader;
			//_setTexCalc._resourceCalcKernel = _computeShader.FindKernel("CSBitToTex");

			//_resMapCalc = new ResourceMapCalculate(_computeShader.FindKernel("CSMain"));
			//_resMapCalc._computeShader = _computeShader;

			gameObject.AddComponent<ResourceMapCalculate>();
			_resMapCalc = GetComponent<ResourceMapCalculate>();
			_resMapCalc._computeShader = _computeShader;
			_resMapCalc._ResourceCalcKernel = _computeShader.FindKernel("CSMain");

			//_terMapCalc = new TerritoriumMapCalculate(_computeShader.FindKernel("CSTerritorium"));
			//_terMapCalc._computeShader = _computeShader;

			gameObject.AddComponent<TerritoriumMapCalculate>();
			_terMapCalc = GetComponent<TerritoriumMapCalculate>();
			_terMapCalc._computeShader = _computeShader;
			_terMapCalc._TerCalcKernel = _computeShader.FindKernel("CSTerritorium");
			_terMapCalc._earlyCalcKernel = _computeShader.FindKernel("CSInit");

			Texture resTex = _GroundMaterial.GetTexture("_NoiseMap");
			Texture terTex = _GroundMaterial.GetTexture("_TerritorriumMap");

			_ResultTextureRessource = new RenderTexture(resTex.width, resTex.height, 0, RenderTextureFormat.RFloat)
			{
				enableRandomWrite = true
			};

			_ResultTextureTerritorium = new RenderTexture(terTex.width, terTex.height, 0, RenderTextureFormat.ARGB32)
			{
				enableRandomWrite = true
			};

			_ResultMyTextureTerritorium = new RenderTexture(terTex.width, terTex.height, 0, RenderTextureFormat.ARGB32)
			{
				enableRandomWrite = true
			};

			_ResultTextureRessource.Create();
			_ResultTextureTerritorium.Create();
			_ResultMyTextureTerritorium.Create();

			Graphics.Blit(resTex, _ResultTextureRessource);
			Graphics.Blit(terTex, _ResultTextureTerritorium);
			Graphics.Blit(terTex, _ResultMyTextureTerritorium);

			_GroundMaterial.SetTexture("_NoiseMap", _ResultTextureRessource);
			_GroundMaterial.SetTexture("_TerritorriumMap", _ResultTextureTerritorium);

			// set fog TeamColors
			_FogMaterial.SetTexture("_TerTex", _ResultMyTextureTerritorium);

			if(GlobalVariables.s_instance._clients.Count > 0)
			{
			//	int team = GlobalVariables.s_instance._clients[0]._id;
				//_FogMaterial.SetColor("TeamColor", GlobalVariables.s_instance._teamColors[team]);
			}
		}


		public void SetRendererTextures(float[] Resfloats, float[] Terfloats)
		{
			//_setTexCalc.StartComputeShader(Resfloats, Terfloats, _ResultTextureRessource, _ResultTextureTerritorium);
			_setTexCalc.StartComputeShader(Resfloats, Terfloats, _ResultTextureRessource, _ResultTextureTerritorium, _ResultMyTextureTerritorium);
		}

		//public void EarlyCalc()
		//{
		//	_earlyCalc.EarlyCalulation(_Refinerys.Count);
		//}

		public void StartHeatMapCalc()
		{
#if DB_HM
			print("startHeatMapCalc");
#endif

			//// for testing 

			//Graphics.Blit(_ResultTerritoriumtest, _ResultTextureTerritorium);
			//// end testing

			StartCoroutine(_resMapCalc.RefreshCalcRes(this));
			StartCoroutine(_terMapCalc.RefreshCalcTerritorium(this));
			//StartCoroutine(test());
		}

		//IEnumerator test()
		//{
		//	yield return new WaitForSecondsRealtime(0.1f);

		//	HeatMapReturnValue[] holder = new HeatMapReturnValue[2];
		//	holder = HeatMapCalcRoutine.s_instance.ReturnValue();
		//	_setTexCalc.StartComputeShader(holder[0].tex, holder[1].tex, _Resulttest, _ResultTerritoriumtest);

		//	_GroundMaterial.SetTexture("_TerritorriumMap", _ResultTerritoriumtest);

		//}

		public HeatMapReturnValue[] ReturnValue()
		{
			HeatMapReturnValue[] map = new HeatMapReturnValue[2];
			map[0] = _resMapCalc.GetValues();
			map[1] = _terMapCalc.GetValues();

			return map;
		}

		public int GetHeatmapWidth(int index)
		{
			switch(index)
			{
				case 0:
					return startResMap.width;
				case 1:
					return startTerMap.width;
				default:
					return -1;
			}
		}

		public void AddFabric(IRefHolder refHolder)
		{
			if(refHolder._Type != ObjectType.REFINERY && !_Refinerys.Contains(refHolder))
			{
				return;
			}
#if DB_HM
			print("add fabric");
#endif
			_Refinerys.Add(refHolder);
		}

		public bool HasRefinerys()
		{
			return _Refinerys.Count != 0;
		}

		// add soldies in List
		public int AddSoldier(Transform pawn, int Team)
		{
			//		print("add Soldiers");
			if(!_Soldiers.ContainsKey(pawn))
			{
				_Soldiers[pawn] = Team;
				return _Soldiers.Count - 1;
			}
			return -1;
		}

		public void RemoveSoldiers(Transform pawn, int index)
		{
			_Soldiers.Remove(pawn);
		}

		public bool HasSoldiers()
		{
			//print("soldier cound " + _Soldiers.Count);
			return _Soldiers.Count != 0;
		}


		[SerializeField] private Texture2D map1;
		[SerializeField] private Texture2D map2;

		public float[][] GetStartArrays()
		{
			Color[] resPi = map1.GetPixels();
			Color[] terPi = map2.GetPixels();

			float[][] textures = new float[2][];

			for(int i = 0; i < textures.Length; i++)
			{
				textures[i] = new float[resPi.Length];
			}

			for(int i = 0; i < resPi.Length; i++)
			{
				textures[0][i] = resPi[i].r;
				textures[1][i] = terPi[i].r;
			}

			return textures;
		}

	}
}