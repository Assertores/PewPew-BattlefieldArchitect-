﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PPBA
{
	public interface IRefHolder
	{
		GameObject _blueprintObj { get; }
		GameObject _GhostPrefabObj { get; }
		Sprite _Image { get; }
		TextMeshProUGUI _ToolTipFeld { get; }
		ObjectType _Type { get; }

		float GetBuildingCost();
		float GetBuildingCurrentResources();
		float GetBuildingTime();
		ObjectType GetObjectType();
		Material GetMaterial();

		Vector4 GetShaderProperties();
	}
}


