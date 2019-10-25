using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RessourceManager : MonoBehaviour
{

	RefineryScript[] allRefineries;

	public void AddRessourcesToRefineries(int[] ressources)
	{
		for (int i = 0; i < allRefineries.Length; i++)
		{
			if (i < ressources.Length)
			{
				allRefineries[i].RessourceStorage = ressources[i];
			}
		}
	}


}
