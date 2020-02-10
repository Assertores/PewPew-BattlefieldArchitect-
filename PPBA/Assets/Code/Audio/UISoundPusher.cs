using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class UISoundPusher : MonoBehaviour
	{
		public void PushClick() => UIBoomboxController.PlayUI(ClipsUI.ICON_CLICK_01);
		

		public void PushSound(ClipsUI clip) => UIBoomboxController.PlayUI(clip);
	}
}