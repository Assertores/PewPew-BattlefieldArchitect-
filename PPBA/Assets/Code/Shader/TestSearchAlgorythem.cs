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

		int maxIndex = int.MaxValue;

		float startcolor = texture.GetPixel(position.x, position.y).r;

		for(int column = 1; column * column < foundDist; column++)
		{
			int rowLength = Mathf.Min(column, maxIndex);
			for(int row = 0; row <= rowLength; row++)
			{
				Vector2Int pos = new Vector2Int();
				bool valueIsDifferent = false;

				#region Check 8 positions

				if(startcolor != texture.GetPixel(position.x + column, position.y + row).r)
				{
					pos.x = position.x + column;
					pos.y = position.y + row;
					valueIsDifferent = true;
				}
				else if(startcolor != texture.GetPixel(position.x - column, position.y + row).r)
				{
					pos.x = position.x - column;
					pos.y = position.y + row;
					valueIsDifferent = true;
				}
				else if(startcolor != texture.GetPixel(position.x + column, position.y - row).r)
				{
					pos.x = position.x + column;
					pos.y = position.y - row;
					valueIsDifferent = true;
				}
				else if(startcolor != texture.GetPixel(position.x - column, position.y - row).r)
				{
					pos.x = position.x - column;
					pos.y = position.y - row;
					valueIsDifferent = true;
				}
				else if(startcolor != texture.GetPixel(position.x + row, position.y + column).r)
				{
					pos.x = position.x + row;
					pos.y = position.y + column;
					valueIsDifferent = true;
				}
				else if(startcolor != texture.GetPixel(position.x - row, position.y + column).r)
				{
					pos.x = position.x - row;
					pos.y = position.y + column;
					valueIsDifferent = true;
				}
				else if(startcolor != texture.GetPixel(position.x + row, position.y - column).r)
				{
					pos.x = position.x + row;
					pos.y = position.y - column;
					valueIsDifferent = true;
				}
				else if(startcolor != texture.GetPixel(position.x - row, position.y - column).r)
				{
					pos.x = position.x - row;
					pos.y = position.y - column;
					valueIsDifferent = true;
				}

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

		foundDist = Mathf.Sqrt((float)foundDist);

		print(texture.GetPixel(position.x, position.y).r);
		print(texture.GetPixel(found.x, found.y).r);
		print(found);
		print(foundDist);
	}
}
