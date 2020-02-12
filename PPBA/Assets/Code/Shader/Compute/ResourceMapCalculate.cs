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

		public int _ResourceCalcKernel;
		public int _EarlyCalcKernel;

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

		private void Awake()
		{
			_backingTex[0] = new float[256 * 256];
			_backingTex[1] = new float[256 * 256];
		}

		public void Start()
		{
			//_resourceCalcKernel1 = kernel;

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
			List<IRefHolder> refHolder = new List<IRefHolder>();

			Vector4[] refProp = RefineriesProperties(HMroutine, ref refHolder);

			if(refProp.Length <= 0)
			{
				goto endingCo;
			}

			isRunning = true;

			// Calculate
			_buffer = new ComputeBuffer(refProp.Length, sizeof(int));

			_bitField = new ComputeBuffer(((256 * 256) / 8 / sizeof(int)), sizeof(int));
			_RedValueBuffer = new ComputeBuffer((256 * 256), sizeof(float));

			_ResourceValues = new int[refProp.Length];
			_currentBitField = new byte[(256 * 256) / 8];
			_RedValues = new float[256 * 256];

			_computeShader.SetBuffer(_EarlyCalcKernel, "buffer", _buffer);
			_computeShader.SetBuffer(_EarlyCalcKernel, "bitField", _bitField);
			_computeShader.Dispatch(_EarlyCalcKernel, 256 / 8, 256 / 8, 1);

			_computeShader.SetInt("PointSize", refProp.Length);
			_computeShader.SetVectorArray("coords", refProp);

			_computeShader.SetBuffer(_ResourceCalcKernel, "bitField", _bitField);
			_computeShader.SetBuffer(_ResourceCalcKernel, "buffer", _buffer);
			_computeShader.SetBuffer(_ResourceCalcKernel, "redValue", _RedValueBuffer);
			_computeShader.SetTexture(_ResourceCalcKernel, "input", HMroutine._ResultTextureRessource);
			_computeShader.SetTexture(_ResourceCalcKernel, "Result", _tempRenTex);

			//_computeShader.SetTexture(_resourceCalcKernel1, "InputTexture", inputTex);

			_computeShader.Dispatch(_ResourceCalcKernel, 256 / 8, 256 / 8, 1);

			yield return new WaitForSeconds(0.05f);

			Graphics.Blit(_tempRenTex, HMroutine._ResultTextureRessource);

			_buffer.GetData(_ResourceValues);
			_bitField.GetData(_currentBitField);
			_RedValueBuffer.GetData(_RedValues);

			for(int i = 0; i < _ResourceValues.Length; i++)
			{
				refHolder[i]._LogicObj.GetComponent<ResourceDepot>().GiveResources((int)(_ResourceValues[i] * 0.01f));
			}

			_bitField.SetCounterValue(0);
			_buffer.SetCounterValue(0);
			_RedValueBuffer.SetCounterValue(0);

			_buffer.Release();
			_bitField.Release();
			_RedValueBuffer.Release();


			_buffer = null;
			_bitField = null;
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

		private Vector4[] RefineriesProperties(HeatMapCalcRoutine HTR, ref List<IRefHolder> refHolder)
		{
			List<Vector4> refsProp = new List<Vector4>();

			foreach(var item in HTR._Refinerys)
			{
				ResourceDepot resHolder = item._LogicObj.GetComponent<ResourceDepot>();

				if(resHolder.HaveResourceSpace())
				{
					refsProp.Add(item.GetShaderProperties);
					refHolder.Add(item);
				}
			}

			return refsProp.ToArray();
		}
	}
}

