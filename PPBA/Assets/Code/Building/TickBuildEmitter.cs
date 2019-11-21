using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class TickBuildEmitter : MonoBehaviour
	{
		public ObjectType _type;

		private void Start()
		{
			int id = TickHandler.s_interfaceInputState.AddObj(_type,transform.position,transform.eulerAngles.y);
			TickEventManager.s_instance.SendBuilding(id);
		}


	}
}