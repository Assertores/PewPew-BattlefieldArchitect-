﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{

	public class BuildServerActivation : MonoBehaviour, INetElement
	{
		public int _id { get; set; }

		private void Awake()
		{
#if !UNITY_SERVER
			TickHandler.s_DoInput += HandleGameStateEnableEvents;
#else
			TickHandler.s_GatherValues += ServerGatherValue;
#endif
		}
		private void OnDestroy()
		{
#if !UNITY_SERVER
			TickHandler.s_DoInput -= HandleGameStateEnableEvents;
#else
			TickHandler.s_GatherValues -= ServerGatherValue;
#endif
		}

		void ServerGatherValue(int tick)
		{
			{
				GSC.transform element = new GSC.transform();
				element._id = _id;
				element._position = transform.position;
				element._angle = transform.rotation.eulerAngles.y;
				TickHandler.s_interfaceGameState._transforms.Add(element);
			}
			{
				GSC.arg element = new GSC.arg();
				element._id = _id;
				if(gameObject.activeSelf)
					element._arguments |= Arguments.ENABLED;

				TickHandler.s_interfaceGameState._args.Add(element);
			}
		}

		//alles was in der funktion drüber in den Gamestate geschrieben wird, bekommt man in der funktion drunter wieder aus dem Gamestate raus
		void HandleGameStateEnableEvents(int tick)
		{
			GSC.arg args = TickHandler.s_interfaceGameState._args.Find(x => x._id == _id);

			if(args != null && args._arguments.HasFlag(Arguments.ENABLED))
			{
				if(!this.gameObject.activeSelf)
				{
					gameObject.SetActive(true);

					GSC.transform newTransform = TickHandler.s_interfaceGameState._transforms.Find(x => x._id == _id);
					if(newTransform != null)
					{
						transform.position = newTransform._position;
						transform.rotation = Quaternion.Euler(0, newTransform._angle, 0);
					}
				}
			}
			else
			{
				if(this.gameObject.activeSelf)
				{
					gameObject.SetActive(false);
				}
			}
		}
	}
}