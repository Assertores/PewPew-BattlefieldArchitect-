using UnityEngine;
using TMPro;

namespace PPBA
{
	public interface IRefHolder
	{
		int _team { get; set; }
		GameObject _LogicObj { get; }
		GameObject _GhostPrefabObj { get; }
		Sprite _Image { get; }
		GameObject _UIElement { get; }
		TextMeshProUGUI _ToolTipFeld { get; }
		ObjectType _Type { get; }

		ObjectType GetObjectType();
		Material GetMaterial();

		Vector4 GetShaderProperties { get; set; }
	}
}


