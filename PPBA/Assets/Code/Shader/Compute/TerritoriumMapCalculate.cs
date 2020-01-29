using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace PPBA
{
	public class TerritoriumMapCalculate
	{
		public ComputeShader _computeShader;
		private ComputeBuffer _bitFieldBuffer;
		private ComputeBuffer _terValueBuffer;
		private byte[] _currentBitField;

		private int _resourceCalcKernel;
		private int _resourceCalcKernel2;

		private float[] _TerValues;
		private bool isRunning = false;

		public HeatMapReturnValue GetValues()
		{
			HeatMap.tex = _backingTex[0];
			return HeatMap;
		}
		private HeatMapReturnValue HeatMap;

		float[][] _backingTex = new float[2][];


		public TerritoriumMapCalculate(int kernel)
		{
			_resourceCalcKernel = kernel;

			_backingTex[0] = new float[256 * 256];
			_backingTex[1] = new float[256 * 256];

			HeatMap = new HeatMapReturnValue
			{
				tex = _backingTex[0],
				bitfield = new byte[(256 * 256) / 8],
			};
		}

		public IEnumerator RefreshCalcTerritorium(HeatMapCalcRoutine HMCalcRoutine)
		{
			if(isRunning || !HMCalcRoutine.HasSoldiers())
			{
				goto endingCo;
			}
			isRunning = true;

			_bitFieldBuffer = new ComputeBuffer(((256 * 256) / 8 / sizeof(uint)), sizeof(uint));
			_terValueBuffer = new ComputeBuffer((256 * 256), sizeof(float));
			_currentBitField = new byte[(256 * 256) / 8];
			_TerValues = new float[256 * 256];

			_computeShader.SetBuffer(_resourceCalcKernel, "bitField", _bitFieldBuffer);
			_computeShader.SetBuffer(_resourceCalcKernel, "terValue", _terValueBuffer);

			_computeShader.SetInt("SoldiersSize", HMCalcRoutine._Soldiers.Count);
			_computeShader.SetVectorArray("Soldiers", AddSoldierData(HMCalcRoutine));
			_computeShader.SetTexture(_resourceCalcKernel, "TerritoriumResult", HMCalcRoutine._ResultTextureTerritorium);

			_computeShader.Dispatch(_resourceCalcKernel, 256 / 8, 256 / 8, 1);

			_bitFieldBuffer.GetData(_currentBitField);
			_bitFieldBuffer.Release();
			_bitFieldBuffer = null;

			_terValueBuffer.GetData(_TerValues);
			_terValueBuffer.Release();
			_terValueBuffer = null;

			string g = _TerValues[500].ToString();

			yield return ConvertRenToTex2D(_currentBitField, _TerValues);
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


		// add data for ComputeInput
		private Vector4[] AddSoldierData(HeatMapCalcRoutine HMCalcRoutine)
		{
			Vector4[] refsProp = new Vector4[HMCalcRoutine._Soldiers.Count];

			int index = 0;

			foreach(KeyValuePair<Transform, int> soldier in HMCalcRoutine._Soldiers)
			{
				Vector2 pos = UserInputController.s_instance.GetTexturePixelPoint(soldier.Key);
				
				refsProp[index] = new Vector4(pos.x, pos.y, soldier.Value, 0);
				index++;
			}
			return refsProp;
		}
	}
}

