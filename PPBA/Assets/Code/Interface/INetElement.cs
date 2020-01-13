using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public interface INetElement
	{
		int _id { get; set; }

		//ALSO IMPLEMENT:
		//Arguments field for trigger flags
		//ClearFlags() -> SetUp(Server)
		//WriteToGameState() -> s_GatherValues(Server)
		//ExtractFromGameState() -> s_DoInput(Client)
	}
}