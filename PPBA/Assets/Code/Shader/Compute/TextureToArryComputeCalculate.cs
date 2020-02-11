using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class TextureToArryComputeCalculate
	{
		public ComputeShader _computeShader;
		private ComputeBuffer _terValueBuffer;

		public int _TexToArrayCalcKernel;

		public Vector4[] colorVecs;

		private float[] _TerValues;
		private bool isRunning = false;

		float[][] _backingTex = new float[2][];

		public HeatMapReturnValue GetValues()
		{
			HeatMap.tex = _backingTex[0];
			return HeatMap;
		}
		private HeatMapReturnValue HeatMap;


		public TextureToArryComputeCalculate(int kernel, Color[] colors)
		{
			_TexToArrayCalcKernel = kernel;
			colorVecs = colors.ColorToVector4();
			_backingTex[0] = new float[256 * 256];
			_backingTex[1] = new float[256 * 256];

			HeatMap = new HeatMapReturnValue
			{
				tex = _backingTex[0],
				bitfield = new byte[(256 * 256) / 8],
			};

		}

		float[] Swap()
		{
			float[] tmp = _backingTex[0];
			_backingTex[0] = _backingTex[1];
			_backingTex[1] = tmp;
			return _backingTex[0];
		}

		public IEnumerator TextureToArrayCalc(HeatMapCalcRoutine HMCalcRoutine)
		{
			Debug.Log("jojojojojojojo");

			if(isRunning)
			{
				goto endingCo;
			}
			isRunning = true;

			_TerValues = new float[256 * 256];
			_terValueBuffer = new ComputeBuffer((256 * 256), sizeof(float));

			_computeShader.SetVectorArray("teamColors", colorVecs);
			_computeShader.SetBuffer(_TexToArrayCalcKernel, "terValue", _terValueBuffer);

			_computeShader.SetTexture(_TexToArrayCalcKernel, "TerritoriumResult", HMCalcRoutine._ResultTextureTerritorium);
			_computeShader.SetTexture(_TexToArrayCalcKernel, "MyTerritoriumResult", HMCalcRoutine._ResultMyTextureTerritorium);
			
			_computeShader.Dispatch(_TexToArrayCalcKernel, 256 / 8, 256 / 8, 1);

			_terValueBuffer.GetData(_TerValues);
			_terValueBuffer.Release();
			_terValueBuffer = null;

			Debug.Log("crazy " + _TerValues[10]);
			yield return ConvertRenToTex2D(_TerValues);

			Swap();

			isRunning = false;
endingCo:
			;
			yield return null;
		}

		IEnumerator ConvertRenToTex2D(float[] renTex)
		{
			_backingTex[1] = renTex;
			yield return null;
		}


	}
}


