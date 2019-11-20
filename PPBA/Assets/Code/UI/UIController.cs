using UnityEngine;
using DG.Tweening;


public class UIController : MonoBehaviour
{
	public RectTransform MainBuildPanel;
	public RectTransform buildUI_1;
	public RectTransform buildUI_2;

	private bool _isOpen1 = false;
	private bool _isOpen2 = false;
	private bool _isMainPanel = false;


	public void MoveMainPanel()
	{
		if (_isMainPanel)
		{
			MainBuildPanel.DOAnchorPos(new Vector2(-MainBuildPanel.rect.x, 120), 0.25f);
		}
		else
		{
			MainBuildPanel.DOAnchorPos(new Vector2(-MainBuildPanel.rect.x, 320), 0.25f);
		}

		_isMainPanel = !_isMainPanel;
	}

	public void MoveBuild1Panel()
	{
		if (!_isMainPanel)
		{
			MoveMainPanel();
		}

		if (_isOpen1)
		{
			buildUI_1.DOAnchorPos(new Vector2(0, -350), 0.25f);
		}
		else
		{
			buildUI_1.DOAnchorPos(new Vector2(0, 0), 0.25f);
			if (_isOpen2)
			{
				buildUI_2.DOAnchorPos(new Vector2(0, -350), 0.25f);
				_isOpen2 = !_isOpen2;
			}
		}

		_isOpen1 = !_isOpen1;

		if (!_isOpen1 && !_isOpen2)
		{
			MoveMainPanel();
		}
	}


	public void MoveBuild2Panel()
	{
		if (!_isMainPanel)
		{
			MoveMainPanel();
		}

		if (_isOpen2)
		{
			buildUI_2.DOAnchorPos(new Vector2(0, -350), 0.25f);
		}
		else
		{
			buildUI_2.DOAnchorPos(new Vector2(0, 0), 0.25f);
			if (_isOpen1)
			{
				buildUI_1.DOAnchorPos(new Vector2(0, -350), 0.25f);
				_isOpen1 = !_isOpen1;

			}
		}

		_isOpen2 = !_isOpen2;

		if (!_isOpen1 && !_isOpen2)
		{
			MoveMainPanel();
		}

	}

	private void CloseAll()
	{
		buildUI_1.DOAnchorPos(new Vector2(0, 0), 0.25f);
		buildUI_2.DOAnchorPos(new Vector2(0, 0), 0.25f);
		MainBuildPanel.DOAnchorPos(new Vector2(-MainBuildPanel.rect.x, 320), 0.25f);

	}

}
