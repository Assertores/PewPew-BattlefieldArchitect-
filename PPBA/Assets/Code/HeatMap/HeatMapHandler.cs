using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//#define UNITY_SERVER
namespace PPBA
{
	public struct HeatMapReturnValue
	{
		public Texture2D tex;
		public byte[] bitfield;
	}

	public class HeatMapHandler : Singleton<HeatMapHandler>
	{
		public Texture2D[] _heatMaps = new Texture2D[2];
		//private BitField2D[] _bitFields = new BitField2D[2];

		private void Start()
		{
			_heatMaps[0] = ResourceMapCalculate.s_instance.GetStartTex();
			_heatMaps[1] = TerritoriumMapCalculate.s_instance.GetStartTex();

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

		void CalculateMaps(int tick)
		{
			ResourceMapCalculate.s_instance.StartCalculation();
			TerritoriumMapCalculate.s_instance.StartCalculation();
		}

		void SaveMapToGameState(int tick)
		{
			HeatMapReturnValue value;
			value = ResourceMapCalculate.s_instance.GetValues();
			TickHandler.s_interfaceGameState._heatMaps.Add(HMRetToGSC(0, ref value));
			_heatMaps[0] = value.tex;

			value = TerritoriumMapCalculate.s_instance.GetValues();
			TickHandler.s_interfaceGameState._heatMaps.Add(HMRetToGSC(1, ref value));
			_heatMaps[1] = value.tex;
		}

		GSC.heatMap HMRetToGSC(int id, ref HeatMapReturnValue input)
		{
			GSC.heatMap value = new GSC.heatMap();

			value._id = id;

			value._mask = new BitField2D(input.tex.width, input.tex.height, input.bitfield);

			Vector2Int[] positions = value._mask.GetActiveBits();

			value._values = new List<float>(positions.Length);
			for(int j = 0; j < positions.Length; j++)
			{
				value._values.Add(input.tex.GetPixel(positions[j].x, positions[j].y).r);
			}

			return value;
		}

		void SetMap(int tick)
		{
			foreach(var it in TickHandler.s_interfaceGameState._heatMaps)
			{
				Vector2Int[] pos = it._mask.GetActiveBits();
				if(pos.Length != it._values.Count)
					throw new System.ArgumentException();

				for(int i = 0; i < pos.Length; i++)
				{
					var tmp = _heatMaps[it._id].GetPixel(pos[i].x, pos[i].y);
					tmp.r = it._values[i];
					_heatMaps[it._id].SetPixel(pos[i].x, pos[i].y, tmp);
				}
				_heatMaps[it._id].Apply();

				switch(it._id)
				{
					case 0:
						ResourceMapCalculate.s_instance.UpdateTexture(_heatMaps[it._id]);
						break;
					case 1:
						TerritoriumMapCalculate.s_instance.UpdateTexture(_heatMaps[it._id]);
						break;
					default:
						Debug.LogError("Heatmap index not found");
						break;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="id">the id of the heatMap</param>
		/// <returns>size of the heatMap</returns>
		public Vector2Int GetHeatMapSize(int id) => new Vector2Int(_heatMaps[id].width, _heatMaps[id].height);

#if Obsolide
		void CalculateMaps(int tick)
		{
			//----- ----- init ----- -----
			HeatMapReturnValue value;

			//----- ----- ResourceMap ----- -----
			value = ResourceMapCalculate.s_instance.RefreshCalcRes();
			_heatMaps[0] = ConvertTexture(value.tex, 0);
			_bitFields[0] = new BitField2D(value.tex.width, value.tex.height, value.bitfield);

			//----- ----- TerritoriumMap ----- -----
			value = TerritoriumMapCalculate.s_instance.RefreshCalcTerritorium();
			_heatMaps[1] = ConvertTexture(value.tex, 1);
			_bitFields[1] = new BitField2D(value.tex.width, value.tex.height, value.bitfield);

		}

		void SaveMapToGameState(int tick)
		{

			for(int i = 0; i < _heatMaps.Length; i++)
			{
				GSC.heatMap hm = new GSC.heatMap();
				hm._id = i;

				//setting up current Bitfield
				hm._mask = new BitField2D(_bitFields[i]);

				Vector2Int[] positions = hm._mask.GetActiveBits();

				//saving values

				hm._values = new List<float>(positions.Length);
				for(int j = 0; j < positions.Length; j++)
				{
					hm._values.Add(_heatMaps[i].GetPixel(positions[j].x, positions[j].y).r);
				}
			//	Debug.Log("Tick: " + tick + ", id: " + hm._id);
			//	Debug.Log(hm.ToString());
				TickHandler.s_interfaceGameState._heatMaps.Add(hm);
			}
		}

		void SetMap(int tick)
		{
			foreach(var it in TickHandler.s_interfaceGameState._heatMaps)
			{
				Vector2Int[] pos = it._mask.GetActiveBits();
				for(int i = 0; i < pos.Length; i++)
				{
					var tmp = _heatMaps[it._id].GetPixel(pos[i].x, pos[i].y);
					tmp.r = it._values[i];
					_heatMaps[it._id].SetPixel(pos[i].x, pos[i].y, tmp);
				}
				_heatMaps[it._id].Apply();

				switch(it._id)
				{
					case 0:
						ResourceMapCalculate.s_instance.UpdateTexture(_heatMaps[it._id]);
						break;
					case 1:
						TerritoriumMapCalculate.s_instance.UpdateTexture(_heatMaps[it._id]);
						break;
					default:
						Debug.LogError("Heatmap index not found");
						break;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="id">the id of the heatMap</param>
		/// <returns>size of the heatMap</returns>
		public Vector2Int GetHeatMapSize(int id) => new Vector2Int(_heatMaps[id].width, _heatMaps[id].height);

		Texture2D ConvertTexture(RenderTexture rt, int id)
		{
			Texture2D tex = _heatMaps[id];

			// ofc you probably don't have a class that is called CameraController :P
			//	Camera activeCamera = Camera.main;

			//	// Initialize and render
			//	activeCamera.targetTexture = rt;
			//	activeCamera.Render();
			//	RenderTexture.active = rt;

			//	// Read pixels
			//	tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
			//	tex.Apply();
			//	// Clean up
			//	activeCamera.targetTexture = null;
			//	RenderTexture.active = null; // added to avoid errors 
			////	DestroyImmediate(rt);

			RenderTexture.active = rt;
			tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
			tex.Apply();

			return tex;

		}
#endif
	}
}
