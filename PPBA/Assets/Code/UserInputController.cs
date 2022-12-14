using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class UserInputController : Singleton<UserInputController>
	{
		private bool _changeMap = false;
		public LayerMask ignore;
		public Material TerrainMat;
		public Terrain _terrain;
		[SerializeField] Terrain _heatMapTerrain;


		private float _ppu;

		private void Start()
		{
			Vector3 size = _terrain.terrainData.size;
			int width = HeatMapCalcRoutine.s_instance.GetHeatmapWidth(0);
			 _ppu = width / size.x;

			ChangeMap(0);
		}


		public Vector3 GetWorldPoint()
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			RaycastHit hitInfo;
			if(Physics.Raycast(ray, out hitInfo, 1000, ignore))
			{
				return hitInfo.point;
			}
			return Vector3.zero;
		}

		private void Update()
		{
			// for test ( ask rene )
			if(Input.GetMouseButtonDown(0))
			{
				GetTexturePixelPoint();
			}
		}

		public Vector2 GetTexturePixelPoint()
		{
			if(null == Camera.main)
			{
				return Vector2.zero;
			}

			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			Vector2 pixelUV;

			RaycastHit hitInfo;
			if(Physics.Raycast(ray, out hitInfo, 1000, ignore))
			{
				pixelUV = hitInfo.textureCoord;
				pixelUV.x = Mathf.FloorToInt(pixelUV.x *= TerrainMat.GetTexture("_NoiseMap").width);
				pixelUV.y = Mathf.FloorToInt(pixelUV.y *= TerrainMat.GetTexture("_NoiseMap").height);
				return pixelUV;
			}
			return Vector3.zero;
		}

		public Vector2 GetTexturePixelPoint(Transform Obj)
		{
			//Vector2 pixelUV;

			//RaycastHit hitInfo;
			//if(Physics.Raycast(Obj.position + Vector3.up, Vector3.down, out hitInfo, 1000, ignore))
			//{
			//	pixelUV = hitInfo.textureCoord;
			//	pixelUV.x = Mathf.FloorToInt(pixelUV.x *= TerrainMat.GetTexture("_NoiseMap").width);
			//	pixelUV.y = Mathf.FloorToInt(pixelUV.y *= TerrainMat.GetTexture("_NoiseMap").height);
			//	return pixelUV;
			//}
			//return Vector3.zero;

			return GetTexturePixelPoint(Obj.position);
		}

		//public Vector2 GetTexturePixelPoint(Vector3 pos)
		//{
		//	Vector2 pixelUV;

		//	RaycastHit hitInfo;
		//	if(Physics.Raycast(pos + Vector3.up, Vector3.down, out hitInfo, 1000, ignore))
		//	{
		//		pixelUV = hitInfo.textureCoord;
		//		pixelUV.x = Mathf.FloorToInt(pixelUV.x *= TerrainMat.GetTexture("_NoiseMap").width);
		//		pixelUV.y = Mathf.FloorToInt(pixelUV.y *= TerrainMat.GetTexture("_NoiseMap").height);
		//		return pixelUV;
		//	}
		//	return Vector3.zero;
		//}

		public void ChangeMap(int index)
		{
			TerrainMat.SetFloat("_MapChange", index);
			if(index == 0)
			{
				_terrain.gameObject.SetActive(true);
				_heatMapTerrain.gameObject.SetActive(false);
			}
			else
			{
				_terrain.gameObject.SetActive(false);
				_heatMapTerrain.gameObject.SetActive(true);
			}
		}


		public Vector2Int GetTexturePixelPoint (Vector3 worldPos)
		{
			Vector3 retPos = (worldPos - _terrain.transform.position) * _ppu;
			return new Vector2Int((int)retPos.x, (int)retPos.z);
			 
		}

		public Vector3 TexPointToWorldSpace(Vector2Int texPos)
		{
			return new Vector3((texPos.x / _ppu) + _terrain.transform.position.x, 0, (texPos.y / _ppu) + _terrain.transform.position.z);
		}

	}
}