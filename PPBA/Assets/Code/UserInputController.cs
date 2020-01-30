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

		public Vector2 GetTexturePixelPoint()
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			Vector2 pixelUV;

			RaycastHit hitInfo;
			if(Physics.Raycast(ray, out hitInfo, 1000, ignore))
			{
				pixelUV = hitInfo.textureCoord;
				pixelUV.x = Mathf.FloorToInt(pixelUV.x *= hitInfo.transform.GetComponent<Renderer>().material.GetTexture("_NoiseMap").width);
				pixelUV.y = Mathf.FloorToInt(pixelUV.y *= hitInfo.transform.GetComponent<Renderer>().material.GetTexture("_NoiseMap").height);
				return pixelUV;
			}
			return Vector3.zero;
		}

		public Vector2 GetTexturePixelPoint(Transform Obj)
		{
			Vector2 pixelUV;

			RaycastHit hitInfo;
			if(Physics.Raycast(Obj.position + Vector3.up, Vector3.down, out hitInfo, 1000, ignore))
			{
				pixelUV = hitInfo.textureCoord;
				pixelUV.x = Mathf.FloorToInt(pixelUV.x *= TerrainMat.GetTexture("_NoiseMap").width);
				pixelUV.y = Mathf.FloorToInt(pixelUV.y *= TerrainMat.GetTexture("_NoiseMap").height);
				return pixelUV;
			}
			return Vector3.zero;
		}

		public Vector2 GetTexturePixelPoint(Vector3 pos)
		{
			Vector2 pixelUV;

			RaycastHit hitInfo;
			if(Physics.Raycast(pos + Vector3.up, Vector3.down, out hitInfo, 1000, ignore))
			{
				pixelUV = hitInfo.textureCoord;
				pixelUV.x = Mathf.FloorToInt(pixelUV.x *= TerrainMat.GetTexture("_NoiseMap").width);
				pixelUV.y = Mathf.FloorToInt(pixelUV.y *= TerrainMat.GetTexture("_NoiseMap").height);
				return pixelUV;
			}
			return Vector3.zero;
		}

		public void ChangeMap(int index)
		{
			TerrainMat.SetFloat("_MapChange", index);
		}


	}
}