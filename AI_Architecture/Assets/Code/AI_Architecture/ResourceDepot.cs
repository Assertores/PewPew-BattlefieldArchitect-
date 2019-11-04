using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceDepot : MonoBehaviour
{
    [SerializeField] public int team;
    [SerializeField] public int resources;
    [SerializeField] public int maxResources;
    [SerializeField] public float score;


    // Start is called before the first frame update
    void Start()
    {
        if (!JobCenter.s_resourceDepots[team].Contains(this))
            JobCenter.s_resourceDepots[team].Add(this);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public float CalculateScore()
    {
        score = 0;

        //determine proximity and weight of build jobs
        foreach (Blueprint b in JobCenter.s_blueprints[team])
        {
            score += b.resourcesNeeded / Vector3.Magnitude(b.transform.position - transform.position);
        }

        return Mathf.Clamp(score, 0f, 1f);
    }

	public int TakeResources(int amount)
	{
		if (amount <= resources)//can take full wanted amount
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

		if (amount <= spaceLeft)//enough space
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

    private void OnEnable()
    {
        if (!JobCenter.s_resourceDepots[team].Contains(this))
            JobCenter.s_resourceDepots[team].Add(this);
    }

    private void OnDisable()
    {
        if (JobCenter.s_resourceDepots[team].Contains(this))
            JobCenter.s_resourceDepots[team].Remove(this);
    }
}
