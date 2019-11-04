using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Behavior_GetResources : Behavior
{
    public static Behavior_GetResources instance;
    public static Dictionary<Pawn, ResourceDepot> targetDictionary;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void Execute(Pawn pawn)
    {
        pawn.navMeshAgent.SetDestination(targetDictionary[pawn].transform.position);
    }

    public override float FindBestTarget(Pawn pawn)
    {
        float _bestScore = 0;

        foreach (ResourceDepot _depot in JobCenter.s_resourceDepots[pawn.team])
        {
            float _tempScore = CalculateTargetScore(pawn, _depot);

            if (_bestScore < _tempScore)
            {
                targetDictionary[pawn] = _depot;
                _bestScore = _tempScore;
            }
        }

        return _bestScore;
    }

    protected override float PawnAxisInputs(Pawn pawn, string name)
    {
        switch (name)
        {
            case "Health":
                return pawn.health / pawn.maxHealth;
            default:
                Debug.LogWarning("PawnAxisInputs defaulted to 1. Probably messed up the string name: " + name);
                return 1;
        }
    }

    protected float TargetAxisInputs(Pawn pawn, string name, ResourceDepot depot)
    {
        switch (name)
        {
            case "Distance":
                return Vector3.Distance(pawn.transform.position, depot.transform.position) / 60f;
            case "Score":
                return depot.score;
            default:
                Debug.LogWarning("TargetAxisInputs defaulted to 1. Probably messed up the string name: " + name);
                return 1;
        }
    }

    protected float CalculateTargetScore(Pawn pawn, ResourceDepot depot)
    {
        float _score = 1;

        for (int i = 0; i < target_axes.Length; i++)
        {
            if (target_axes[i].isEnabled)
                _score *= Mathf.Clamp(target_axes[i].curve.Evaluate(TargetAxisInputs(pawn, target_axes[i].name, depot)), 0f, 1f);
        }

        return _score;
    }
}
