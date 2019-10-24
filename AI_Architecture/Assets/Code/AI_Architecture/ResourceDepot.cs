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
        if(!JobCenter.resourceDepots[team].Contains(this))
        JobCenter.resourceDepots[team].Add(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public float CalculateScore()
    {
        score = 0;

        //determine proximity and weight of build jobs
        foreach (Blueprint b in JobCenter.blueprints[team])
        {
            score += b.resourcesNeeded / Vector3.Magnitude(b.transform.position - transform.position);
        }

        return Mathf.Clamp(score, 0f, 1f);
    }

    private void OnEnable()
    {
        if (!JobCenter.resourceDepots[team].Contains(this))
            JobCenter.resourceDepots[team].Add(this);
    }

    private void OnDisable()
    {
        if (JobCenter.resourceDepots[team].Contains(this))
            JobCenter.resourceDepots[team].Remove(this);
    }
}
