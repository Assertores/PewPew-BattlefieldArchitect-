using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RessourceManager : MonoBehaviour
{
	private List<RefineryScript> refinerys = new List<RefineryScript>();

	public void AddRefinery(RefineryScript refi)
	{
		refinerys.Add(refi);
	}


	public void AddRessourcesToRefineries(int[] ressources)
	{
		for (int i = 0; i < refinerys.Count; i++)
		{
			if (i < ressources.Length)
			{
				refinerys[i].RessourceStorage = ressources[i];
			}
		}
	}


}
