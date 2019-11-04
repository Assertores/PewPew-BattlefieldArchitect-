﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class JobCenter : MonoBehaviour
	{
		public static JobCenter instance;

		public static List<Blueprint>[] s_blueprints = new List<Blueprint>[10];//can exchange 10 for number of players
		public static List<ResourceDepot>[] s_resourceDepots = new List<ResourceDepot>[10];

		void Awake()
		{
			if(instance == null)
				instance = this;
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