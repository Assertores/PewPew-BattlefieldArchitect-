using UnityEngine;
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

		ObjectType GetObjectType();
		Material GetMaterial();

		Vector4 GetShaderProperties { get; set; }
	}
}


