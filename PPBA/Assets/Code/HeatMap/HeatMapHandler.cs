using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public struct HeatMapReturnValue
	{
		public RenderTexture tex;
		public byte[] bitfield;
	}

	public class HeatMapHandler : Singleton<HeatMapHandler>
	{
		Texture2D[] _heatMaps = new Texture2D[2];
		BitField2D[] _bitFields = new BitField2D[2];
		bool _isInit = false;

		private void Start()
		{
#if UNITY_SERVER
			TickHandler.s_DoTick += CalculateMaps;
			TickHandler.s_GatherValues += SaveMapToGameState;
#else
			_heatMaps[0] = ResourceMapCalculate.s_instance.GetStartTex();
			_heatMaps[1] = TerritoriumMapCalculate.s_instance.GetStartTex();

			TickHandler.s_DoInput += SetMap;
#endif
		}
		private void OnDestroy()
		{
#if UNITY_SERVER
			TickHandler.s_DoTick -= CalculateMaps;
			TickHandler.s_GatherValues -= SaveMapToGameState;
#else
			TickHandler.s_DoInput -= SetMap;
#endif
		}

		void CalculateMaps(int tick)
		{
			if(!_isInit)
			{
				if(ResourceMapCalculate.s_instance.HasRefinerys() /*|| TerritoriumMapCalculate.s_instance.HasSoldiers()*/)
				{
					_isInit = true;
				}

				if(!_isInit)
				{
					return;
				}
			}

			HeatMapReturnValue value;

			print("calculate maps Heatmaphandler");

			//----- ----- init ----- -----

			if(ResourceMapCalculate.s_instance.HasRefinerys())
			{
				print("map calc refi");
				//----- ----- ResourceMap ----- -----
				value = ResourceMapCalculate.s_instance.RefreshCalcRes();
				_heatMaps[0] = ConvertTexture(value.tex);
				_bitFields[0] = new BitField2D(value.tex.width, value.tex.height, value.bitfield);				
			}

			if(TerritoriumMapCalculate.s_instance.HasSoldiers())
			{
				print("map calc soldiers");
				////----- ----- TerritoriumMap ----- -----
				value = TerritoriumMapCalculate.s_instance.RefreshCalcTerritorium();
				_heatMaps[1] = ConvertTexture(value.tex);
				print("heatpmap value  " + value.tex);
				_bitFields[1] = new BitField2D(value.tex.width, value.tex.height, value.bitfield);
			}
		}

		void SaveMapToGameState(int tick)
		{
			if(!_isInit)
			{
				Debug.Log("Heatmaps not initialiced yet");
				return;
			}
			for(int i = 0; i < _heatMaps.Length; i++)
			{
				GSC.heatMap hm = new GSC.heatMap();
				hm._id = i;

				//setting up current Bitfield
				hm._mask = _bitFields[i];

				Vector2Int[] positions = hm._mask.GetActiveBits();

				//saving values

				hm._values = new List<float>(positions.Length);
				for(int j = 0; j < positions.Length; j++)
				{
					hm._values.Add(_heatMaps[i].GetPixel(positions[j].x, positions[j].y).r);
				}
				Debug.Log("Tick: " + tick + ", id: " + hm._id);
				Debug.Log(hm.ToString());
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
					_heatMaps[it._id].SetPixel(pos[i].x, pos[i].y, new Color(it._values[i], 0, 0, 0));
				}

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

		Texture2D ConvertTexture(RenderTexture rt)
		{
			Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);

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
	}
}
