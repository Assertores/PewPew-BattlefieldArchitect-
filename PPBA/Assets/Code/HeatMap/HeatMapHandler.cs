using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//#define UNITY_SERVER
namespace PPBA
{
	public struct HeatMapReturnValue
	{
		public float[] tex;
		public byte[] bitfield;
	}

	public class HeatMapHandler : Singleton<HeatMapHandler>
	{
		public float[][] _heatMaps = new float[2][];
		//private BitField2D[] _bitFields = new BitField2D[2];

		[SerializeField] Terrain _terrain;
		float[] _ppu = new float[2];

		private void Start()
		{
			if(!_terrain)
			{
				Debug.LogError("terain reference not set");
				Destroy(this);
				return;
			}

			Vector3 size = _terrain.terrainData.size;
			for(int i = 0; i < _ppu.Length; i++)
			{
				int width = HeatMapCalcRoutine.s_instance.GetHeatmapWidth(i);
				_ppu[i] = width / size.x;
			}

			var retValue = HeatMapCalcRoutine.s_instance.ReturnValue();

			_heatMaps[0] = retValue[0].tex;
			_heatMaps[1] = retValue[1].tex;

#if UNITY_SERVER
			TickHandler.s_EarlyCalc += CalculateMaps;
			TickHandler.s_GatherValues += SaveMapToGameState;
#else
			TickHandler.s_DoInput += SetMap;
#endif
		}
		private void OnDestroy()
		{
#if UNITY_SERVER
			TickHandler.s_EarlyCalc -= CalculateMaps;
			TickHandler.s_GatherValues -= SaveMapToGameState;
#else
			TickHandler.s_DoInput -= SetMap;
#endif
		}

		Dictionary<Vector2Int, Vector3> h_cashValues = new Dictionary<Vector2Int, Vector3>();

		/// <summary>
		/// calculates position and distance to nearest boarder
		/// </summary>
		/// <param name="worldPos">your world position</param>
		/// <returns>x = target x position in worldspace, y = target y position in worldspace, z = distance to target in worldspace</returns>
		public Vector3 BorderValues(Vector3 worldPos)
		{
			Vector2 retPos = worldPos * _ppu[1];
			Vector2Int texPos = new Vector2Int((int)retPos.x, (int)retPos.y);

			if(h_cashValues.ContainsKey(texPos))
				return h_cashValues[texPos];

			//----- -----> has to be calculated <----- -----

			Vector2Int found = new Vector2Int();
			float foundDist = 150;//searching maximum distance of 150 pixle so if the hole map is from the same team it wond calculate forever

			int maxIndex = int.MaxValue;

			int myTeam = (int)GetHMValue(1, texPos.x, texPos.y);

			for(int column = 1; column * column < foundDist; column++)
			{
				int rowLength = Mathf.Min(column, maxIndex);
				for(int row = 0; row <= rowLength; row++)
				{
					Vector2Int pos = new Vector2Int();
					bool valueIsDifferent = false;

					#region Check 8 positions

					pos = texPos + new Vector2Int(column, row);
					if(!valueIsDifferent && myTeam != (int)GetHMValue(1, pos.x, pos.y))
						valueIsDifferent = true;

					pos = texPos + new Vector2Int(-column, row);
					if(!valueIsDifferent && myTeam != (int)GetHMValue(1, pos.x, pos.y))
						valueIsDifferent = true;

					pos = texPos + new Vector2Int(column, -row);
					if(!valueIsDifferent && myTeam != (int)GetHMValue(1, pos.x, pos.y))
						valueIsDifferent = true;

					pos = texPos + new Vector2Int(-column, -row);
					if(!valueIsDifferent && myTeam != (int)GetHMValue(1, pos.x, pos.y))
						valueIsDifferent = true;

					pos = texPos + new Vector2Int(row, column);
					if(!valueIsDifferent && myTeam != (int)GetHMValue(1, pos.x, pos.y))
						valueIsDifferent = true;

					pos = texPos + new Vector2Int(-row, column);
					if(!valueIsDifferent && myTeam != (int)GetHMValue(1, pos.x, pos.y))
						valueIsDifferent = true;

					pos = texPos + new Vector2Int(row, -column);
					if(!valueIsDifferent && myTeam != (int)GetHMValue(1, pos.x, pos.y))
						valueIsDifferent = true;

					pos = texPos + new Vector2Int(-row, -column);
					if(!valueIsDifferent && myTeam != (int)GetHMValue(1, pos.x, pos.y))
						valueIsDifferent = true;

					#endregion

					if(valueIsDifferent)
					{
						int dist = column * column + row * row;
						if(foundDist > dist)
						{
							found = pos;
							maxIndex = row;
							foundDist = dist;
						}
					}
				}
			}

			if(150 <= foundDist)
			{
				found = new Vector2Int(Random.Range(0, HeatMapCalcRoutine.s_instance.GetHeatmapWidth(1)), Random.Range(0, HeatMapCalcRoutine.s_instance.GetHeatmapWidth(1)));
				foundDist = found.x * found.x + found.y * found.y;
			}

			foundDist = Mathf.Sqrt(foundDist);

			h_cashValues[texPos] = new Vector3(found.x / _ppu[1], found.y / _ppu[1], foundDist / _ppu[1]);

			return h_cashValues[texPos];
		}

		void CalculateMaps(int tick)
		{

			if(tick % 5 == 0)
			{
				//HeatMapCalcRoutine.s_instance.EarlyCalc();
				HeatMapCalcRoutine.s_instance.StartHeatMapCalc();

			}
		}

		void SaveMapToGameState(int tick)
		{
			h_cashValues.Clear();

			HeatMapReturnValue[] value;
			value = HeatMapCalcRoutine.s_instance.ReturnValue();

			TickHandler.s_interfaceGameState.Add(HMRetToGSC(0, ref value[0]));
			_heatMaps[0] = value[0].tex;

			TickHandler.s_interfaceGameState.Add(HMRetToGSC(0, ref value[1]));
			_heatMaps[1] = value[1].tex;
		}

		GSC.heatMap HMRetToGSC(int id, ref HeatMapReturnValue input)
		{
			BitField2D mask = new BitField2D(HeatMapCalcRoutine.s_instance.GetHeatmapWidth(id), HeatMapCalcRoutine.s_instance.GetHeatmapWidth(id), input.bitfield);//TODO: get Hight from texture
			Vector2Int[] pos = mask.GetActiveBits();


			GSC.heatMap value = new GSC.heatMap();
			value._id = id;

			value._values = new List<GSC.heatMapElement>(pos.Length);
			for(int i = 0; i < pos.Length; i++)
			{
				GSC.heatMapElement element = new GSC.heatMapElement();

				element._x = (byte)pos[i].x;
				element._y = (byte)pos[i].y;
				element._value = input.tex[pos[i].x + pos[i].y * HeatMapCalcRoutine.s_instance.GetHeatmapWidth(id)];

				value._values.Add(element);
			}

			return value;
		}

		void SetMap(int tick)
		{
			for(int id = 0; id < _heatMaps.Length; id++)
			{
				GSC.heatMap map = TickHandler.s_interfaceGameState.GetHeatMap(id);
				if(null == map || null == map._values)
					continue;

				foreach(var it in map._values)
				{
					_heatMaps[id][it._x + it._y * HeatMapCalcRoutine.s_instance.GetHeatmapWidth(id)] = it._value;
				}
			}
			//HeatMapCalcRoutine.s_instance.SetRendererTextures(_heatMaps[0], _heatMaps[1]);
		}

		int[] h_widths = new int[2];
		int[] h_hights = new int[2];
		float GetHMValue(int id, int x, int y)
		{
			if(id < 0 || id >= _heatMaps.Length)
				return float.NaN;

			if(h_widths[id] == 0)
			{
				h_widths[id] = HeatMapCalcRoutine.s_instance.GetHeatmapWidth(id);
				h_hights[id] = HeatMapCalcRoutine.s_instance.GetHeatmapWidth(id);//TODO: get Hight from texture
			}

			return _heatMaps[id][Lib.Mod(x, h_widths[id]) + Lib.Mod(y, h_hights[id]) * h_widths[id]];
		}
	}
}
