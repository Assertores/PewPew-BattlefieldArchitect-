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
		
		private Texture2D inputTex;

		private ComputeBuffer _bitField;
		private byte[] _currentBitField;

		private int _resourceCalcKernel;
		private int _resourceCalcKernel2;

		private Dictionary<Transform, int> _Soldiers = new Dictionary<Transform, int>();


		private bool isRunning = false;

		public HeatMapReturnValue GetValues()
		{
			HeatMap.tex = _backingTex[0];
			return HeatMap;
		}
		private HeatMapReturnValue HeatMap;

		float[][] _backingTex = new float[2][];

		float[] Swap()
		{
			float[] tmp = _backingTex[0];
			_backingTex[0] = _backingTex[1];
			_backingTex[1] = tmp;
			return _backingTex[0];
		}



		void Start()
		{
			if(_GroundMaterial == null)
			{
				Debug.LogError("No Ground Material Found!!! (ResourceMapCalculate)");
				return;
			}

			Texture2D resourceTexture = _original;
			//Texture2D resourceTexture = Instantiate(_GroundMaterial.GetTexture("_TerritorriumMap")) as Texture2D;
			_resourceCalcKernel = _computeShader.FindKernel("CSTerritorium");

			_ResultTexture = new RenderTexture(resourceTexture.width, resourceTexture.height, 0, RenderTextureFormat.ARGB32)
			{
				enableRandomWrite = true
			};

			_ResultTexture.Create();
			Graphics.Blit(resourceTexture, _ResultTexture);

			inputTex = new Texture2D(256, 256, TextureFormat.ARGB32, false);
			Graphics.CopyTexture(_ResultTexture, inputTex);

			_GroundMaterial.SetTexture("_TerritorriumMap", _ResultTexture);

			_backingTex[0] = new float[256 * 256];
			_backingTex[1] = new float[256 * 256];

			HeatMap = new HeatMapReturnValue
			{
				tex = _backingTex[0],
				bitfield = new byte[(256 * 256) / 8],
			};


		}

		public Texture2D GetStartTex() => _original;

		public void UpdateTexture(Texture2D newTexture)
		{
			_GroundMaterial.SetTexture("_TerritorriumMap", newTexture);
		}

		public void StartCalculation()
		{
			if(isRunning)
			{
				return;
			}

			StartCoroutine(RefreshCalcTerritorium());
		}

		public IEnumerator RefreshCalcTerritorium()
		{
			isRunning = true;

			if(!HasSoldiers())
			{
				yield return null;
			}

			_bitField = new ComputeBuffer(((256 * 256) / 8 / sizeof(uint)), sizeof(uint));
			_currentBitField = new byte[(256 * 256) / 8];

			_computeShader.SetBuffer(_resourceCalcKernel, "bitField", _bitField);

			_computeShader.SetInt("SoldiersSize", _Soldiers.Count);
			_computeShader.SetVectorArray("Soldiers", AddSoldierData());
			_computeShader.SetTexture(_resourceCalcKernel, "TerritoriumResult", _ResultTexture);
			_computeShader.SetTexture(_resourceCalcKernel, "InputTextureTerritorium", inputTex);

			_computeShader.Dispatch(_resourceCalcKernel, 256 / 8, 256 / 8, 1);

			_bitField.GetData(_currentBitField);
			_bitField.Release();
			_bitField = null;
			
			//yield return new WaitForEndOfFrame();
			yield return new WaitForSeconds(0.2f);

			_GroundMaterial.SetTexture("_TerritorriumMap", _ResultTexture);

			yield return StartCoroutine(ConvertRenToTex2D(_currentBitField, new float[65000], _ResultTexture));
			Swap(); // change Texture2d

			isRunning = false;
		}

		IEnumerator ConvertRenToTex2D(byte[] field, float[] renTex, RenderTexture tex)
		{
			Graphics.CopyTexture(tex, inputTex);
			HeatMap.bitfield = field;
			_backingTex[1] = renTex;
			yield return null;
		}


		// add data for ComputeInput
		private Vector4[] AddSoldierData()
		{
			Vector4[] refsProp = new Vector4[_Soldiers.Count];

			int index = 0;

			foreach(KeyValuePair<Transform, int> soldier in _Soldiers)
			{
				Vector2 pos = UserInputController.s_instance.GetTexturePixelPoint(soldier.Key);


				refsProp[index] = new Vector4(pos.x, pos.y, soldier.Value, 0);
				index++;
			}
			return refsProp;
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
			//print("soldier cound " + _Soldiers.Count);
			return _Soldiers.Count != 0;
		}

	}

}

