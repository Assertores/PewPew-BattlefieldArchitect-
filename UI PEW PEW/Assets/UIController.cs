using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


public class UIController : MonoBehaviour
{
	public RectTransform MainBuildPanel;
	public RectTransform buildUI_1;
	public RectTransform buildUI_2;
	public float PanelHeight = 300;

	private bool _isOpen1 = false;
	private bool _isOpen2 = false;
	private bool _isMainPanel = false;


	public void MoveBuildsPanel(RectTransform rec, bool status)
	{
		if (!_isMainPanel)
		{
			SwitchBuildPanel(MainBuildPanel, !_isMainPanel);
			_isMainPanel = !_isMainPanel;
		}

		SwitchBuildPanel(rec, status);
	}

	public void SwitchBuildPanel(RectTransform rec, bool dir)
	{
		if (dir)
		{
			rec.DOAnchorPos(new Vector2(0, PanelHeight), 0.25f);
		}
		else
		{
			rec.DOAnchorPos(new Vector2(0, 0), 0.25f);
		}

	}

	public void CloseBuildPanels()
	{
		SwitchBuildPanel(MainBuildPanel, false);
		SwitchBuildPanel(buildUI_1, false);
		SwitchBuildPanel(buildUI_2, false);
	}

	public void SwitchBuild1()
	{
		if (_isOpen1)
		{
			MoveBuildsPanel(buildUI_1, !_isOpen1);
			_isOpen1 = !_isOpen1;
		}
		else
		{
			CloseBuildPanels();
		}
	}

	public void SwitchBuild2()
	{
		if (_isOpen2)
		{
			MoveBuildsPanel(buildUI_1, !_isOpen2);
			_isOpen2 = !_isOpen2;
		}
		else
		{
			CloseBuildPanels();
		}


	}
}
