using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class HeatMapHandler : Singleton<HeatMapHandler>
	{
		/// <summary>
		/// 
		/// </summary>
		/// <returns>all ids of existing HeatMaps</returns>
		public int[] GetAllIds()
		{
			throw new System.NotImplementedException();
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
		public Vector2Int[] GetChangedPositions(int id)
		{
			throw new System.NotImplementedException();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="id">the id of the heatMap</param>
		/// <returns>the texture of the respective heatMap</returns>
		public Texture2D GetHeatMap(int id)
		{
			throw new System.NotImplementedException();
		}
	}
}
