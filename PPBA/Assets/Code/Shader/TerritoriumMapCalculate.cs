using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace PPBA
{
	public class TerritoriumMapCalculate : Singleton<TerritoriumMapCalculate>
	{
		[SerializeField] private ComputeShader _computeShader;
		[SerializeField] private Material _GroundMaterial;
		[SerializeField] Texture2D _original;

		private RenderTexture _ResultTexture;
		private ComputeBuffer _buffer;

		private ComputeBuffer _bitField;
		private byte[] _currentBitField;

		private int _resourceCalcKernel;
		private int _resourceCalcKernel2;

		private Dictionary<Transform, int> _Soldiers = new Dictionary<Transform, int>();

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

		public Texture2D GetStartTex() => _original;

		public void UpdateTexture(Texture2D newTexture)
		{
			_GroundMaterial.SetTexture("_TerritorriumMap", newTexture);

		}

		public HeatMapReturnValue RefreshCalcTerritorium()
		{

			_currentBitField = new byte[(512 * 512) / 8];
			HeatMapReturnValue value;
			value.bitfield = _currentBitField;
			value.tex = _ResultTexture;

			if(!s_instance.HasSoldiers())
			{
				return value;
			}

			_currentBitField = new byte[(512 * 512) / 8];
			_bitField = new ComputeBuffer(((512 * 512) / 8 / sizeof(int)), sizeof(int));

			_computeShader.SetBuffer(_resourceCalcKernel, "bitField", _bitField);

			_computeShader.SetInt("SoldiersSize", _Soldiers.Count);
			_computeShader.SetTexture(_resourceCalcKernel, "TerritoriumResult", _ResultTexture);
			_computeShader.SetVectorArray("Soldiers", AddSoldierData());

			_computeShader.Dispatch(_resourceCalcKernel, 512 / 8, 512 / 8, 1);

			_bitField.GetData(_currentBitField);
			_bitField.Release();
			_bitField = null;

			return new HeatMapReturnValue { tex = _ResultTexture, bitfield = _currentBitField };
		}

		// add data for ComputeInput
		private Vector4[] AddSoldierData()
		{
			Vector4[] refsProp = new Vector4[_Soldiers.Count];

			int index = 0;

			foreach(KeyValuePair<Transform, int> soldier in _Soldiers)
			{
				refsProp[index] = new Vector4(soldier.Key.position.x, soldier.Key.position.y, soldier.Value, 0);
				index++;
			}
			return refsProp;
		}

		void Update()
		{
			print(_Soldiers.Count + " "+ _Soldiers.Keys);
		}

		// add soldies in List
		public int AddSoldier(Transform pawn, int Team)
		{
			print("add Soldiers");
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

		void OnDisable()
		{
			_GroundMaterial.SetTexture("_TerritorriumMap", _original);
		}

		void OnEnable()
		{
			_GroundMaterial.SetTexture("_TerritorriumMap", _original);
		}


		public bool HasSoldiers()
		{
			print("soldier cound " + _Soldiers.Count);
			return _Soldiers.Count != 0;
		}

	}

}

