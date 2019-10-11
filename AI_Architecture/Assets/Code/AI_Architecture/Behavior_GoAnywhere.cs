using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Behavior_GoAnywhere : Behavior
{
    public static Behavior_GoAnywhere instance;

    [SerializeField] private float maxDistance = 30f;
    private Vector3 bestTarget;

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
        pawn.navMeshAgent.SetDestination(bestTarget);
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

    protected override float TargetAxisInputs(Pawn pawn, string name)
    {
        switch (name)
        {
            case "DistanceToTarget":
                return Vector3.Distance(pawn.transform.position, bestTarget) / maxDistance;
            default:
                Debug.LogWarning("TargetAxisInputs defaulted to 1. Probably messed up the string name: " + name);
                return 1;
        }
    }

    public override void FindBestTarget(Pawn pawn)
    {
        bestTarget = GetRandomPoint(pawn);
        bestTarget = pawn.transform.position + Vector3.forward;
        targetScore = 1;
    }

    /// <summary> Gets a random point up to 3 from transform.position </summary>
    protected Vector3 GetRandomPoint(Pawn pawn)
    {
        UnityEngine.AI.NavMeshHit hit;
        Vector2 probe;
        Vector3 probePosition;

        for (int i = 0; i < 64; i++)
        {
            probe = Random.insideUnitCircle * 3f;
            probePosition = new Vector3(transform.position.x + probe.x, transform.position.y, transform.position.z + probe.y);

            if (UnityEngine.AI.NavMesh.SamplePosition(probePosition, out hit, 0.1f, pawn.navMeshAgent.areaMask))
                return hit.position;
            else
            {   //checks the same point in the opposite direction
                probePosition = new Vector3(-probePosition.x, probePosition.y, -probePosition.z);
                if (UnityEngine.AI.NavMesh.SamplePosition(probePosition, out hit, 0.1f, pawn.navMeshAgent.areaMask))
                    return hit.position;
            }
        }

        return transform.position;
    }
}
