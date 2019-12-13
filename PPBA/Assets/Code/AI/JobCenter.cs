using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class JobCenter : MonoBehaviour
	{
		public static JobCenter s_instance;

		public static List<Blueprint>[] s_blueprints = new List<Blueprint>[10];//can exchange 10 for number of players
		public static List<ResourceDepot>[] s_resourceDepots = new List<ResourceDepot>[10];
		public static List<MountSlot>[] s_mountSlots = new List<MountSlot>[10];
		public static List<CoverSlot>[] s_coverSlots = new List<CoverSlot>[10];
		public static List<FlagPole>[] s_flagPoles = new List<FlagPole>[10];
		public static List<HeadQuarter>[] s_headQuarters = new List<HeadQuarter>[10];

		void Awake()
		{
			if(s_instance == null)
				s_instance = this;
			else
			{
				Destroy(gameObject);
				return;
			}

			for(int i = 0; i < s_blueprints.Length; i++)
			{
				if(s_blueprints[i] == null)
					s_blueprints[i] = new List<Blueprint>();
				if(s_resourceDepots[i] == null)
					s_resourceDepots[i] = new List<ResourceDepot>();
				if(s_mountSlots[i] == null)
					s_mountSlots[i] = new List<MountSlot>();
				if(s_coverSlots[i] == null)
					s_coverSlots[i] = new List<CoverSlot>();
				if(s_flagPoles[i] == null)
					s_flagPoles[i] = new List<FlagPole>();
			}
		}

		void Start()
		{

		}

		void Update()
		{

		}
	}
}