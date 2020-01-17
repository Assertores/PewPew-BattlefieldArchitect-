using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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
		public static List<MediCamp>[] s_mediCamp = new List<MediCamp>[10];
		public static List<HeadQuarter>[] s_headQuarters = new List<HeadQuarter>[10];

		#region Monobehaviour
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
				if(s_mediCamp[i] == null)
					s_mediCamp[i] = new List<MediCamp>();
				if(s_headQuarters[i] == null)
					s_headQuarters[i] = new List<HeadQuarter>();
			}
		}

		void Start()
		{

		}

		void Update()
		{
#if false
			LogPolesPerTeam();
#endif
		}
		#endregion

		public static void ChangeTeamList<T>(List<T>[] list, T item, int newTeam)
		{
			bool isInRightTeam = false;

			for(int i = 0; i < list.Length; i++)
			{
				if(i == newTeam && list[i].Contains(item))
					isInRightTeam = true;
				else if(list[i].Contains(item))
					list[i].Remove(item);
			}

			if(!isInRightTeam)
				list[newTeam].Add(item);
		}

		private void LogPolesPerTeam()
		{
			for(int i = 0; i < 2; i++)
			{
				string temp = "poles team " + i.ToString() + ": ";

				foreach(var pole in s_flagPoles[i])
				{
					temp += " " + pole._id.ToString() + ",";
				}

				Debug.Log(temp);
			}
		}

		public static void CheckWinCon()
		{
			bool[] areWinners = new bool[s_headQuarters.Length];
			int alivePlayers = 0;

			for(int i = 0; i < s_headQuarters.Length; i++)
			{
				if(0 < s_headQuarters[i].Count)
				{
					alivePlayers++;
					areWinners[i] = true;
				}
				else
					areWinners[i] = false;
			}

			if(alivePlayers == 1)
			{
				for(int i = 0; i < areWinners.Length; i++)
				{
					if(areWinners[i])
						StatusNetcode.s_instance.SetWinningConndition(i);
				}
			}
		}
	}
}