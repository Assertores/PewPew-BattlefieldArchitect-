using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class BuildingColorSetter : Singleton<BuildingColorSetter>
	{
		static public void SetMaterialColor(Renderer renderer, MaterialPropertyBlock _propertyBlock, int team = 0)
		{
			if(null == renderer || null == _propertyBlock)
				return;

			Color color;

			if(team < GlobalVariables.s_instance._teamColors.Length)
				color = GlobalVariables.s_instance._teamColors[team];
			else
				switch(team)
				{
					case 0:
						color = Color.green;
						break;
					case 1:
						color = Color.blue;
						break;
					case 2:
						color = Color.yellow;
						break;
					case 3:
						color = Color.red;
						break;
					default:
						color = Color.gray;
						break;
				}

			renderer.GetPropertyBlock(_propertyBlock);
			_propertyBlock.SetColor("_Emission", color);
			renderer.SetPropertyBlock(_propertyBlock);
		}
	}
}