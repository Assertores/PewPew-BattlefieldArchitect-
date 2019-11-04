using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blueprint : MonoBehaviour
{
    [SerializeField] public int team = 0;
    [SerializeField] public int resources = 0;
    [SerializeField] public int resourcesIncoming = 0;
    [SerializeField] public int resourcesMax = 100;
    [SerializeField] public int work = 0;
    [SerializeField] public int workMax = 50;

    [SerializeField] public List<Pawn> workers = new List<Pawn>();

    public float workDoable
    {
        get => ((float)(resources * resourcesMax) / (float)workMax) - (float)work;
    }

    public float resourcesNeeded
    {
        get => (float)(resourcesMax - resources - resourcesIncoming);
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!JobCenter.s_blueprints[team].Contains(this))
            JobCenter.s_blueprints[team].Add(this);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void WorkTick()
    {
        foreach (Pawn w in workers)
        {
            work += 1;
        }

        if (workMax <= work)
            WorkIsFinished();
    }

    private void WorkIsFinished()
    {
		//exchange blueprint for building
    }

    private void OnEnable()
    {
        if (!JobCenter.s_blueprints[team].Contains(this))
            JobCenter.s_blueprints[team].Add(this);
    }

    private void OnDisable()
    {
        if (JobCenter.s_blueprints[team].Contains(this))
            JobCenter.s_blueprints[team].Remove(this);
    }
}
