using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class TerritoriumMapCalculate : Singleton<TerritoriumMapCalculate>
	{
		public ComputeShader _computeShader;
		public Material _GroundMaterial;

		public Texture _TerritorriumMap;
		private RenderTexture _ResultTexture;
		private ComputeBuffer _buffer;

		private ComputeBuffer _bitField;
		private byte[] _currentBitField;

		private int _resourceCalcKernel;
		private int _resourceCalcKernel2;

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
		
		public HeatMapReturnValue RefreshCalcTerritorium()
		{
			_currentBitField = new byte[(512 * 512) / 8];
			_bitField = new ComputeBuffer(((512 * 512) / 8 / sizeof(int)), sizeof(int));

			_computeShader.SetBuffer(_resourceCalcKernel, "resourcesIndex", _bitField);
			
			_computeShader.SetInt("SoldiersSize", _Soldiers.Count);
			_computeShader.SetTexture(_resourceCalcKernel, "TerritoriumResult", _ResultTexture);
			_computeShader.SetVectorArray("Soldiers", AddSoldierData());
			
			_computeShader.Dispatch(_resourceCalcKernel, 512 / 8, 512 / 8, 1);

			_GroundMaterial.SetTexture("_TerritorriumMap", _ResultTexture);

			_bitField.GetData(_currentBitField);
			_bitField.Release();
			_bitField = null;

			return new HeatMapReturnValue { tex = _ResultTexture , bitfield = _currentBitField };
		}

		private void SendToTickManager()
		{

			//	ressourceManager.AddRessourcesToRefineries(CurrentValue);
		}


		// add data for ComputeInput
		private Vector4[] AddSoldierData()
		{
			Vector4[] refsProp = new Vector4[_Soldiers.Count];

			for(int i = 0; i < _Soldiers.Count; i++)
			{
				refsProp[i] = new Vector4(_Soldiers[i].x , _Soldiers[i].y, _Soldiers[i].z, 0);
			}
			return refsProp;
		}

		// add soldies in List
		public int AddSoldier(Vector2 pos, int Team)
		{
			_Soldiers.Add(new Vector4(pos.x , pos.y, Team ));
			return _Soldiers.Count-1;
		}

		public void UpdateSoldiersPosition(int index, Vector2 position)
		{
			_Soldiers[index] = new Vector4(position.x, position.y, _Soldiers[index].z, 0);
		}



		void OnDisable()
		{
			_GroundMaterial.SetTexture("_TerritorriumMap", _TerritorriumMap);
		}
	}

}

