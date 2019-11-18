using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public interface IPanelInfo
	{
		void GetPanelInfo();//TODO: Adjust return type to Rene's PanelViewer and let all clickable objects implement this interface.
	}
}