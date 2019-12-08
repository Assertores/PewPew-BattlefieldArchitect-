using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSearchAlgorythem : MonoBehaviour
{
	[Header("In")]
	[SerializeField] Texture2D texture;
	[SerializeField] Vector2Int position;

	[Header("Out")]
	[SerializeField] Vector2Int found;
	[SerializeField] float foundDist;

	private void Start()
	{
		StarSerach();
	}

	public void StarSerach()
	{
		foundDist = float.MaxValue;

		int row = int.MaxValue;

		float startcolor = texture.GetPixel(position.x, position.y).r;

		for(int i = 1; i * i < foundDist; i++)
		{
			int h = Mathf.Min(i, row);
			for(int j = 0; j < h; j++)
			{
				Vector2Int pos = new Vector2Int();
				bool doCheck = false;

				#region Check 8 positions

				if(startcolor != texture.GetPixel(position.x + i, position.y + j).r)
				{
					pos.x = position.x + i;
					pos.y = position.y + j;
					doCheck = true;
				}
				else if(startcolor != texture.GetPixel(position.x - i, position.y + j).r)
				{
					pos.x = position.x - i;
					pos.y = position.y + j;
					doCheck = true;
				}
				else if(startcolor != texture.GetPixel(position.x + i, position.y - j).r)
				{
					pos.x = position.x + i;
					pos.y = position.y - j;
					doCheck = true;
				}
				else if(startcolor != texture.GetPixel(position.x - i, position.y - j).r)
				{
					pos.x = position.x - i;
					pos.y = position.y - j;
					doCheck = true;
				}
				else if(startcolor != texture.GetPixel(position.x + j, position.y + i).r)
				{
					pos.x = position.x + j;
					pos.y = position.y + i;
					doCheck = true;
				}
				else if(startcolor != texture.GetPixel(position.x - j, position.y + i).r)
				{
					pos.x = position.x - j;
					pos.y = position.y + i;
					doCheck = true;
				}
				else if(startcolor != texture.GetPixel(position.x + j, position.y - i).r)
				{
					pos.x = position.x + j;
					pos.y = position.y - i;
					doCheck = true;
				}
				else if(startcolor != texture.GetPixel(position.x - j, position.y - i).r)
				{
					pos.x = position.x - j;
					pos.y = position.y - i;
					doCheck = true;
				}

				#endregion

				if(doCheck)
				{
					int dist = i * i + j * j;
					if(foundDist > dist)
					{
						found = pos;
						row = j;
						foundDist = dist;
					}
				}
			}
		}

		foundDist = Mathf.Sqrt((float)foundDist);

		print(texture.GetPixel(position.x, position.y).r);
		print(texture.GetPixel(found.x, found.y).r);
		print(found);
		print(foundDist);
	}
}
