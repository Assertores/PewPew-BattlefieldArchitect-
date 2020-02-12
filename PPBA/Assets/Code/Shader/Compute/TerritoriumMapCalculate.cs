using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace PPBA
{
	public class TerritoriumMapCalculate : MonoBehaviour
	{
		public ComputeShader _computeShader;
		private ComputeBuffer _buffer;
		private ComputeBuffer _bitFieldBuffer;
		private ComputeBuffer _terValueBuffer;
		private byte[] _currentBitField;

		public int _TerCalcKernel;
		public int _earlyCalcKernel;

		private float[] _TerValues;
		private bool isRunning = false;

		public HeatMapReturnValue GetValues()
		{
			HeatMap.tex = _backingTex[0];
			return HeatMap;
		}
		private HeatMapReturnValue HeatMap;

		float[][] _backingTex = new float[2][];

		private void Awake()
		{
			_backingTex[0] = new float[256 * 256];
			_backingTex[1] = new float[256 * 256];
		}

		public void Start()
		{
			//_resourceCalcKernel = kernel;

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

			_buffer = new ComputeBuffer(HMCalcRoutine._Soldiers.Count, sizeof(int));
			_bitFieldBuffer = new ComputeBuffer(((256 * 256) / 8 / sizeof(uint)), sizeof(uint));
			_terValueBuffer = new ComputeBuffer((256 * 256), sizeof(float));
			_currentBitField = new byte[(256 * 256) / 8];
			_TerValues = new float[256 * 256];

			_computeShader.SetBuffer(_earlyCalcKernel, "buffer", _buffer);
			_computeShader.SetBuffer(_earlyCalcKernel, "bitField", _bitFieldBuffer);
			_computeShader.Dispatch(_earlyCalcKernel, 256 / 8, 256 / 8, 1);
			
			_computeShader.SetBuffer(_TerCalcKernel, "bitField", _bitFieldBuffer);
			_computeShader.SetBuffer(_TerCalcKernel, "terValue", _terValueBuffer);

			_computeShader.SetInt("SoldiersSize", HMCalcRoutine._Soldiers.Count);
			_computeShader.SetVectorArray("Soldiers", AddSoldierData(HMCalcRoutine));
			_computeShader.SetTexture(_TerCalcKernel, "TerritoriumResult", HMCalcRoutine._ResultTextureTerritorium);

			_computeShader.Dispatch(_TerCalcKernel, 256 / 8, 256 / 8, 1);

			_buffer.Release();

			_bitFieldBuffer.GetData(_currentBitField);
			_bitFieldBuffer.Release();
			_bitFieldBuffer = null;

			_terValueBuffer.GetData(_TerValues);
			_terValueBuffer.Release();
			_terValueBuffer = null;

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

		public Vector4[] h_solgerArry;
		// add data for ComputeInput
		private Vector4[] AddSoldierData(HeatMapCalcRoutine HMCalcRoutine)
		{
			Vector4[] refsProp = new Vector4[HMCalcRoutine._Soldiers.Count];

			int index = 0;

			foreach(KeyValuePair<Transform, soldierStruct> soldier in HMCalcRoutine._Soldiers)
			{
				Vector2Int pos = UserInputController.s_instance.GetTexturePixelPoint(soldier.Key.position);
				
				refsProp[index] = new Vector4(pos.x, pos.y, soldier.Value.Team, soldier.Value.CicleLenght);
				index++;
			}

			h_solgerArry = refsProp;
			return refsProp;
		}
	}
}

