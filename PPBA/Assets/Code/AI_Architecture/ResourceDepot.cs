using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class ResourceDepot : MonoBehaviour
	{
		[SerializeField] public int team;
		[SerializeField] public int resources;
		[SerializeField] public int maxResources;
		[SerializeField] public int ammo;
		[SerializeField] public int maxAmmo;
		[SerializeField] public float score;

		void Start()
		{

		}

		void Update()
		{

		}

		public void CalculateScore(int tick = 0)
		{
			score = 0;

			if(0 == resources)
				return;

			//determine proximity and weight of build jobs
			foreach(Blueprint b in JobCenter.s_blueprints[team])
			{
				//score has to be normalised somehow. what would be a good max?
				score += b.resourcesNeeded / Vector3.Magnitude(b.transform.position - transform.position);
			}

			score = Mathf.Clamp(score, 0f, 1f);
		}

		#region Give Or Take
		public int TakeResources(int amount)
		{
			if(amount <= resources)//can take full wanted amount
			{
				resources -= amount;
				return amount;
			}
			else//can only take a part of the wanted amount
			{
				int temp = resources;
				resources = 0;
				return temp;
			}
		}

		public int GiveResources(int amount)
		{
			int spaceLeft = maxResources - resources;

			if(amount <= spaceLeft)//enough space
			{
				resources += amount;
				return amount;
			}
			else//not enough space
			{
				resources += spaceLeft;
				return spaceLeft;
			}
		}
		public int TakeAmmo(int amount)
		{
			if(amount <= ammo)//can take full wanted amount
			{
				ammo -= amount;
				return amount;
			}
			else//can only take a part of the wanted amount
			{
				int temp = ammo;
				ammo = 0;
				return temp;
			}
		}

		public int GiveAmmo(int amount)
		{
			int spaceLeft = maxAmmo - ammo;

			if(amount <= spaceLeft)//enough space
			{
				ammo += amount;
				return amount;
			}
			else//not enough space
			{
				ammo += spaceLeft;
				return spaceLeft;
			}
		}
		#endregion

		private void OnEnable()
		{
			if(!JobCenter.s_resourceDepots[team].Contains(this))
				JobCenter.s_resourceDepots[team].Add(this);

			TickHandler.s_LateCalc += CalculateScore;
		}

		private void OnDisable()
		{
			if(JobCenter.s_resourceDepots[team].Contains(this))
				JobCenter.s_resourceDepots[team].Remove(this);

			TickHandler.s_LateCalc -= CalculateScore;
		}
	}
}