using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class ResourceMapCalculate : MonoBehaviour
	{
		public ComputeShader _computeShader;
		private ComputeBuffer _buffer;
		private ComputeBuffer _RedValueBuffer;
		private ComputeBuffer _bitField;

		private RenderTexture _tempRenTex;

		public int _resourceCalcKernel1;

		private bool isRunning = false;

		public HeatMapReturnValue GetValues()
		{
			HeatMap.tex = _backingTex[0];
			return HeatMap;
		}

		private HeatMapReturnValue HeatMap;

		float[][] _backingTex = new float[2][];

		[SerializeField]
		private int[] _ResourceValues;
		[SerializeField]
		private float[] _RedValues;

		private byte[] _currentBitField;

		public void Start()
		{
			//_resourceCalcKernel1 = kernel;

			_backingTex[0] = new float[256 * 256];
			_backingTex[1] = new float[256 * 256];

			HeatMap = new HeatMapReturnValue
			{
				tex = _backingTex[0],
				bitfield = new byte[(256 * 256) / 8],
			};

			Texture resTex = HeatMapCalcRoutine.s_instance._GroundMaterial.GetTexture("_NoiseMap");
			_tempRenTex = new RenderTexture(resTex.width, resTex.height, 0, RenderTextureFormat.RFloat)
			{
				enableRandomWrite = true
			};

			Graphics.Blit(HeatMapCalcRoutine.s_instance._ResultTextureRessource, _tempRenTex);


		}

		public IEnumerator RefreshCalcRes(HeatMapCalcRoutine HMroutine)
		{
			if(isRunning || !HMroutine.HasRefinerys())
			{
				goto endingCo;
			}

			isRunning = true;

			_currentBitField = new byte[(256 * 256) / 8];
			_bitField = new ComputeBuffer(((256 * 256) / 8 / sizeof(int)), sizeof(int));
			_ResourceValues = new int[HMroutine._Refinerys.Count];
			_RedValues = new float[256 * 256];

			_buffer = new ComputeBuffer(HMroutine._Refinerys.Count, sizeof(int));

			_RedValueBuffer = new ComputeBuffer((256 * 256), sizeof(float));

			_computeShader.SetInt("PointSize", HMroutine._Refinerys.Count);
			_computeShader.SetVectorArray("coords", RefineriesProperties(HMroutine));

			_computeShader.SetBuffer(_resourceCalcKernel1, "bitField", _bitField);
			_computeShader.SetBuffer(_resourceCalcKernel1, "buffer", _buffer);
			_computeShader.SetBuffer(_resourceCalcKernel1, "redValue", _RedValueBuffer);
			_computeShader.SetTexture(_resourceCalcKernel1, "input", HMroutine._ResultTextureRessource);
			_computeShader.SetTexture(_resourceCalcKernel1, "Result", _tempRenTex);

			//_computeShader.SetTexture(_resourceCalcKernel1, "InputTexture", inputTex);

			_computeShader.Dispatch(_resourceCalcKernel1, 256 / 16, 256 / 16, 1);

			yield return new WaitForSeconds(0.05f);

			Graphics.Blit(_tempRenTex, HeatMapCalcRoutine.s_instance._ResultTextureRessource);

			_bitField.GetData(_currentBitField);
			_bitField.Release();
			_bitField = null;
			_buffer.GetData(_ResourceValues);
			_buffer.Release();
			_buffer = null;
			_RedValueBuffer.GetData(_RedValues);
			_RedValueBuffer.Release();
			_RedValueBuffer = null;

			yield return new WaitForEndOfFrame();
			HMroutine._GroundMaterial.SetTexture("_NoiseMap", HMroutine._ResultTextureRessource);

			yield return ConvertRenToTex2D(_currentBitField, _RedValues);
			Swap(); // change Texture2d
			isRunning = false;
endingCo:
			;
		}

		float[] Swap()
		{
			float[] tmp = _backingTex[0];
			_backingTex[0] = _backingTex[1];
			_backingTex[1] = tmp;
			return _backingTex[0];
		}

		IEnumerator ConvertRenToTex2D(byte[] field, float[] renTex)
		{
			HeatMap.bitfield = field;
			_backingTex[1] = renTex;
			yield return null;
		}

		private Vector4[] RefineriesProperties(HeatMapCalcRoutine HTR)
		{
			Vector4[] refsProp = new Vector4[HTR._Refinerys.Count];

			for(int i = 0; i < HTR._Refinerys.Count; i++)
			{
				refsProp[i] = HTR._Refinerys[i].GetShaderProperties;
			}
			return refsProp;
		}
	}
}

