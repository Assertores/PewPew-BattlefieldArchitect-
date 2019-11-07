using UnityEngine;
using DG.Tweening;

public class UnitScreenController : MonoBehaviour
{

	public RectTransform UnitPanel;

	private bool _isUnitPanel = false;

	public void MoveUnitPanel()
	{
		if (_isUnitPanel)
		{
			UnitPanel.DOAnchorPos(new Vector2(-UnitPanel.rect.x, 400), 0.25f);
		}
		else
		{
			UnitPanel.DOAnchorPos(new Vector2(-UnitPanel.rect.x, 0), 0.25f);
		}

		_isUnitPanel = !_isUnitPanel;
	}
}
