using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavTestPawn : MonoBehaviour
{
    [SerializeField] private GameObject objectToFollow;
    private NavMeshAgent navMeshAgent;

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (objectToFollow != null && navMeshAgent != null)
            navMeshAgent.SetDestination(objectToFollow.transform.position);
    }
}
