using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class ResourceMapCalculate : Singleton<ResourceMapCalculate>
	{

		public ComputeShader _computeShader;
		public Material _GroundMaterial;
	//	public RenderTexture _currentTexture;
		[SerializeField] Texture2D _original;

		private RenderTexture _ResultTexture;
		private ComputeBuffer _buffer;
		private ComputeBuffer _bitField;

		private int _resourceCalcKernel1;
		private int _resourceCalcKernel2;
		private List<IRefHolder> _Refinerys = new List<IRefHolder>();


		[SerializeField]
		private int[] _ResourceValues;
		
		private byte[] _currentBitField;

		
		void Start()
		{
			if(_GroundMaterial == null)
			{
				Debug.LogError("No Ground Material Found!!! (ResourceMapCalculate)");
				return;
			}

			Texture2D resourceTexture = Instantiate(_GroundMaterial.GetTexture("_NoiseMap")) as Texture2D;
			_resourceCalcKernel1 = _computeShader.FindKernel("CSMain");
			_resourceCalcKernel2 = _computeShader.FindKernel("CSInit");

			_ResultTexture = new RenderTexture(resourceTexture.width, resourceTexture.height, 0, RenderTextureFormat.R8)
			{
				enableRandomWrite = true
			};

			_ResultTexture.Create();
			Graphics.Blit(resourceTexture, _ResultTexture);
			//Graphics.Blit(resourceTexture, _currentTexture);
			_GroundMaterial.SetTexture("_NoiseMap", _ResultTexture);

		}

		public Texture2D GetStartTex() => _original;

		public void UpdateTexture(Texture2D newTexture)
		{
			_GroundMaterial.SetTexture("_NoiseMap", newTexture);

		}

		public void AddFabric(IRefHolder refHolder)
		{
			print("add fabric");
			_Refinerys.Add(refHolder);
		}

		public HeatMapReturnValue RefreshCalcRes()
		{
			_currentBitField = new byte[(512 * 512) / 8];
			_bitField = new ComputeBuffer(((512 * 512) / 8 / sizeof(int)), sizeof(int));
			_ResourceValues = new int[_Refinerys.Count];
			_buffer = new ComputeBuffer(_Refinerys.Count, sizeof(int));

			_computeShader.SetInt("PointSize", _Refinerys.Count);

			_computeShader.SetVectorArray("coords", RefineriesProperties());

			_computeShader.SetBuffer(_resourceCalcKernel2, "buffer", _buffer);

			_computeShader.SetBuffer(_resourceCalcKernel1, "bitField", _bitField);
			_computeShader.SetBuffer(_resourceCalcKernel1, "buffer", _buffer);

		//	_computeShader.SetTexture(_resourceCalcKernel1, "InputTexture", _ResultTexture);
			_computeShader.SetTexture(_resourceCalcKernel1, "Result", _ResultTexture);

			_computeShader.Dispatch(_resourceCalcKernel2, 50, 1, 1); // prüfen ob er hier wartet
			_computeShader.Dispatch(_resourceCalcKernel1, 512 / 8, 512 / 8, 1);

			_bitField.GetData(_currentBitField);
			_bitField.Release();
			_bitField = null;
			_buffer.GetData(_ResourceValues);
			_buffer.Release();
			_buffer = null;

		//	Graphics.Blit(_ResultTexture, _currentTexture);

			//_GroundMaterial.SetTexture("_NoiseMap", _ResultTexture);

			return new HeatMapReturnValue { bitfield = _currentBitField, tex = _ResultTexture };

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
			print("property von cacl fabric");
			Vector4[] refsProp = new Vector4[_Refinerys.Count];

			for(int i = 0; i < _Refinerys.Count; i++)
			{
				refsProp[i] = _Refinerys[i].GetShaderProperties;
			}
			return refsProp;
		}

		void OnDisable()
		{
			_GroundMaterial.SetTexture("_NoiseMap", _original);
		}

		void OnEnable()
		{
			_GroundMaterial.SetTexture("_NoiseMap", _original);
		}

		public bool HasRefinerys()
		{
			return _Refinerys.Count != 0;
		}
	}
		
}

