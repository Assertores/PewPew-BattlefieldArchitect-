using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class ResourceMapCalculate : Singleton<ResourceMapCalculate>
	{
		public ComputeShader _computeShader;
		public Material _GroundMaterial;
		[SerializeField] Texture2D _original;

		private Texture2D inputTex;
		private RenderTexture _ResultTexture;
		private ComputeBuffer _buffer;
		private ComputeBuffer _RedValueBuffer;
		private ComputeBuffer _bitField;

		private int _resourceCalcKernel1;
		private int _resourceCalcKernel2;
		private List<IRefHolder> _Refinerys = new List<IRefHolder>();

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

		[SerializeField]
		private int[] _ResourceValues;
		private float[] _RedValues;

		private byte[] _currentBitField;

		void Start()
		{
			if(_GroundMaterial == null)
			{
				Debug.LogError("No Ground Material Found!!! (ResourceMapCalculate)");
				return;
			}

			//Texture2D resourceTexture = Instantiate(_GroundMaterial.GetTexture("_NoiseMap")) as Texture2D;
			Texture2D resourceTexture = _original;
			_resourceCalcKernel1 = _computeShader.FindKernel("CSMain");
			_resourceCalcKernel2 = _computeShader.FindKernel("CSInit");

			_ResultTexture = new RenderTexture(resourceTexture.width, resourceTexture.height, 0, RenderTextureFormat.RFloat)
			{
				enableRandomWrite = true
			};

			_ResultTexture.Create();
			Graphics.Blit(resourceTexture, _ResultTexture);

			inputTex = new Texture2D(256,256, TextureFormat.RFloat, false);
			Graphics.CopyTexture(_ResultTexture, inputTex);

			_backingTex[0] = new float[256 * 256];
			_backingTex[1] = new float[256 * 256];

			_GroundMaterial.SetTexture("_NoiseMap", _ResultTexture);

			HeatMap = new HeatMapReturnValue
			{
				tex = _backingTex[0],
				bitfield = new byte[(256 * 256) / 8],
			};
		}

		public Texture2D GetStartTex() => _original;

		public void UpdateTexture(Texture2D newTexture)
		{
			_GroundMaterial.SetTexture("_NoiseMap", newTexture);
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

		public void StartCalculation()
		{
			print("start calculation!!!");
			if(isRunning || !s_instance.HasRefinerys())
			{
				print("stop calculate ... is running now or has no ref");
				return;
			}

			StartCoroutine(RefreshCalcRes());
		}

		public IEnumerator RefreshCalcRes()
		{
			isRunning = true;


			_currentBitField = new byte[(256 * 256) / 8];
			_bitField = new ComputeBuffer(((256 * 256) / 8 / sizeof(int)), sizeof(int));
			_ResourceValues = new int[_Refinerys.Count];
			_RedValues = new float[256 * 256];

			_buffer = new ComputeBuffer(_Refinerys.Count, sizeof(int));

			_RedValueBuffer = new ComputeBuffer((256 * 256), sizeof(float));

			_computeShader.SetInt("PointSize", _Refinerys.Count);
			_computeShader.SetVectorArray("coords", RefineriesProperties());

			_computeShader.SetBuffer(_resourceCalcKernel2, "buffer", _buffer);
			_computeShader.SetBuffer(_resourceCalcKernel2, "redValue", _RedValueBuffer);

			_computeShader.SetBuffer(_resourceCalcKernel1, "bitField", _bitField);
			_computeShader.SetBuffer(_resourceCalcKernel1, "buffer", _buffer);
			_computeShader.SetBuffer(_resourceCalcKernel1, "redValue", _RedValueBuffer);
			_computeShader.SetTexture(_resourceCalcKernel1, "Result", _ResultTexture);
			_computeShader.SetTexture(_resourceCalcKernel1, "InputTexture", inputTex);

			_computeShader.Dispatch(_resourceCalcKernel2, 256 / 8, 256 / 8, 1); // prüfen ob er hier wartet
			yield return new WaitForSeconds(0.1f);
			_computeShader.Dispatch(_resourceCalcKernel1, 256 / 8, 256 / 8, 1);
			yield return new WaitForSeconds(0.1f);


			_bitField.GetData(_currentBitField);
			_bitField.Release();
			_bitField = null;
			_buffer.GetData(_ResourceValues);
			_buffer.Release();
			_buffer = null;

			_RedValueBuffer.GetData(_RedValues);
			_RedValueBuffer.Release();
			_RedValueBuffer = null;

			//	yield return new WaitForEndOfFrame();
			//	_GroundMaterial.SetTexture("_NoiseMap", _ResultTexture);

			yield return StartCoroutine(ConvertRenToTex2D(_currentBitField, _RedValues, _ResultTexture));
			Swap(); // change Texture2d

			isRunning = false;
			print("end of function");
		}

		IEnumerator ConvertRenToTex2D(byte[] field, float[] renTex, RenderTexture tex)
		{
			Graphics.CopyTexture(tex, inputTex);
			HeatMap.bitfield = field;
			_backingTex[1] = renTex;
			yield return null;
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
			print("refinerys cound " + _Refinerys.Count + (_Refinerys.Count != 0));

			return _Refinerys.Count != 0;
		}
	}

}

