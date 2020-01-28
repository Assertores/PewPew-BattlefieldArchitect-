﻿using System.Collections;
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

		public RenderTexture _ResultTextureRessource;
		public RenderTexture _ResultTextureTerritorium;

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
			//_earlyCalc = new EarlyCalculate(_computeShader.FindKernel("CSInit"));
			//_earlyCalc._computeShader = _computeShader;

			//_setTexCalc = new SetTextureMapCalculate(_computeShader.FindKernel("CSBitToTex"));
			//_setTexCalc._computeShader = _computeShader;

			gameObject.AddComponent<SetTextureMapCalculate>();
			_setTexCalc = GetComponent<SetTextureMapCalculate>();
			_setTexCalc._computeShader = _computeShader;
			_setTexCalc._resourceCalcKernel = _computeShader.FindKernel("CSBitToTex");

			//_resMapCalc = new ResourceMapCalculate(_computeShader.FindKernel("CSMain"));
			//_resMapCalc._computeShader = _computeShader;

			gameObject.AddComponent<ResourceMapCalculate>();
			_resMapCalc = GetComponent<ResourceMapCalculate>();
			_resMapCalc._computeShader = _computeShader;
			_resMapCalc._ResourceCalcKernel = _computeShader.FindKernel("CSMain");

			_terMapCalc = new TerritoriumMapCalculate(_computeShader.FindKernel("CSTerritorium"));
			_terMapCalc._computeShader = _computeShader;

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

			_ResultTextureRessource.Create();
			_ResultTextureTerritorium.Create();

			Graphics.Blit(resTex, _ResultTextureRessource);
			Graphics.Blit(terTex, _ResultTextureTerritorium);

			_GroundMaterial.SetTexture("_NoiseMap", _ResultTextureRessource);
			_GroundMaterial.SetTexture("_TerritorriumMap", _ResultTextureTerritorium);
		}

		public void SetRendererTextures(float[] Resfloats, float[] Terfloats)
		{
			_setTexCalc.StartComputeShader(Resfloats, Terfloats, _ResultTextureRessource, _ResultTextureTerritorium);
		}

		//public void EarlyCalc()
		//{
		//	_earlyCalc.EarlyCalulation(_Refinerys.Count);
		//}

		public void StartHeatMapCalc()
		{
			StartCoroutine(_resMapCalc.RefreshCalcRes(this));
			StartCoroutine(_terMapCalc.RefreshCalcTerritorium(this));
		}

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
			print("add fabric");
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

		public void PrintSomething(string text)
		{
			print("das solltest du lesen! : " + text);
		}

		public float[][] GetStartArrays()
		{
			Color[] resPi = startResMap.GetPixels();
			Color[] terPi = startResMap.GetPixels();

			float[][] textures = new float[2][];

			for(int i = 0; i < resPi.Length; i++)
			{
				textures[0][i] = resPi[i].a;
				textures[1][i] = terPi[i].a;
			}

			return textures;
		}

	}
}