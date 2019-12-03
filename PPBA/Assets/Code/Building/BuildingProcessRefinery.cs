using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PPBA
{
	public class BuildingProcessRefinery : MonoBehaviour, INetElement
	{
		public int index = 0;
		public TextMeshProUGUI text;

		private RefineryRefHolder _holder;
		private bool EnoughResources;

		public int _id { get; set; }

		private void Awake()
		{
			_holder = GetComponent<RefineryRefHolder>();
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

		private void OnEnable()
		{
#if !UNITY_SERVER
			Startbuilding();
#endif
		}

		private void Startbuilding()
		{
			StartCoroutine(StartBuilding());
		}

		IEnumerator StartBuilding()
		{
			yield return new WaitForSeconds(0.1f);

			while(!EnoughResources)
			{
				if(_holder._CurrentResources >= _holder._BuildingCosts)
				{
					EnoughResources = true;
				}
				yield return new WaitForSeconds(1);
			}

			yield return StartCoroutine(BuildingRoutine());

			ResourceMapCalculate.s_instance.AddFabric(GetComponent<RefineryRefHolder>());
			_holder.RefineryPrefab.SetActive(true);

			yield return null;
		}

		IEnumerator BuildingRoutine()
		{

			// TODO BUIlding zyclus  zb Dissolver
			yield return null;

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
				if(_holder.gameObject.activeSelf) 
					element._arguments |= Arguments.ENABLED;

				text.text = index + element._arguments.ToString();
				index++;


				//Debug.Log(element._arguments.ToString() +" <---args!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");

				TickHandler.s_interfaceGameState._args.Add(element);
			}
		}

		//alles was in der funktion drüber in den Gamestate geschrieben wird, bekommt man in der funktion drunter wieder aus dem Gamestate raus
		void HandleGameStateEnableEvents(int tick)
		{
			//print("HandleGameStateEnableEvents --- tick: " + tick);
			GSC.arg args = TickHandler.s_interfaceGameState._args.Find(x => x._id == _id);

			if(args != null)
			{
				//Debug.Log(args._arguments.ToString());

			}

			if(args != null && args._arguments.HasFlag(Arguments.ENABLED))
			{
	//			print("11111111111111111111tick: " + tick + " wir machen mich jetzt an");
				if(!this.gameObject.activeSelf)
				{
			//		print("tick: " + tick + " wir machen mich jetzt an");
					_holder.gameObject.SetActive(true);

					GSC.transform newTransform = TickHandler.s_interfaceGameState._transforms.Find(x => x._id == _id);
					if(newTransform != null)
					{
						_holder.transform.position = newTransform._position;
						_holder.transform.rotation = Quaternion.Euler(0, newTransform._angle, 0);
					}
				}
			}
			else
			{
				if(this.gameObject.activeSelf)
				{
					_holder.gameObject.SetActive(false);
				}
			}
		}
	}
}