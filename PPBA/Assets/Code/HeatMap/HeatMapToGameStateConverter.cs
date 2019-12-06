using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class HeatMapToGameStateConverter : Singleton<HeatMapToGameStateConverter>
	{
		void Start()
		{
#if UNITY_SERVER
			TickHandler.s_GatherValues += Converter;
#endif
		}
		private void OnDestroy()
		{
#if UNITY_SERVER
			TickHandler.s_GatherValues -= Converter;
#endif
		}

		void Converter(int tick)
		{
			foreach(var it in HeatMapHandler.s_instance.GetAllIds())
			{
				GSC.heatMap hm = new GSC.heatMap();
				hm._id = it;

				//setting up current Bitfield
				Vector2Int size = HeatMapHandler.s_instance.GetHeatMapSize(it);
				hm._mask = new BitField2D(size.x, size.y, HeatMapHandler.s_instance.GetBitMap(it));

				Vector2Int[] positions = hm._mask.GetActiveBits();

				//saving values
				Texture2D heatMap = HeatMapHandler.s_instance.GetHeatMap(it);

				hm._values = new List<float>(positions.Length);
				for(int i = 0; i < positions.Length; i++)
				{
					hm._values.Add(heatMap.GetPixel(positions[i].x, positions[i].y).r);
				}

				TickHandler.s_interfaceGameState._heatMaps.Add(hm);
			}
		}
	}
}