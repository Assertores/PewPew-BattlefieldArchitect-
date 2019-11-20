using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PPBA
{
	public interface IUIElement
	{
		GameObject _GhostPrefabObj { get; }
		Sprite _Image { get; }
		TextMeshProUGUI _ToolTipFeld { get; }
		ObjectType _Type { get; }
	}
}


