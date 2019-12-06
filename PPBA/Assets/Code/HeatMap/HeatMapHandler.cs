using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class HeatMapHandler : Singleton<HeatMapHandler>
	{
		RenderTexture[] _heatMaps;
		BitField2D[] _bitFields;

		private void Start()
		{
#if UNITY_SERVER
			TickHandler.s_DoTick += CalculateMaps;
			TickHandler.s_GatherValues += Converter;
#endif
		}
		private void OnDestroy()
		{
#if UNITY_SERVER
			TickHandler.s_DoTick -= CalculateMaps;
			TickHandler.s_GatherValues -= Converter;
#endif
		}

		void CalculateMaps(int tick)
		{
			ResourceMapCalculate.s_instance.RefreshCalcRes();
			TerritoriumMapCalculate.s_instance.RefreshCalcTerritorium();
		}

		void Converter(int tick)
		{
			foreach(var it in GetAllIds())
			{
				GSC.heatMap hm = new GSC.heatMap();
				hm._id = it;

				//setting up current Bitfield
				Vector2Int size = GetHeatMapSize(it);
				hm._mask = GetBitMap(it);

				Vector2Int[] positions = hm._mask.GetActiveBits();

				//saving values
				Texture2D heatMap = GetHeatMap(it);

				hm._values = new List<float>(positions.Length);
				for(int i = 0; i < positions.Length; i++)
				{
					hm._values.Add(heatMap.GetPixel(positions[i].x, positions[i].y).r);
				}

				TickHandler.s_interfaceGameState._heatMaps.Add(hm);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns>all ids of existing HeatMaps</returns>
		public int[] GetAllIds()
		{
			int[] value = new int[_heatMaps.Length];
			for(int i = 0; i < value.Length; i++)
			{
				value[i] = i;
			}
			return value;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="id">the id of the heatMap</param>
		/// <returns>size of the heatMap</returns>
		public Vector2Int GetHeatMapSize(int id)
		{

			throw new System.NotImplementedException();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="id">the id of the heatMap</param>
		/// <returns>the values that have changed in the last tick</returns>
		public BitField2D GetBitMap(int id)
		{
			return _bitFields[id];
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="id">the id of the heatMap</param>
		/// <returns>the texture of the respective heatMap</returns>
		public Texture2D GetHeatMap(int id)
		{
			// pointer auf map
			throw new System.NotImplementedException();
			//return _heatMaps[id];
		}
	}
}
