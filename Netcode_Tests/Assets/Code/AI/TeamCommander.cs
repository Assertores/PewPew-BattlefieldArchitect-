using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TeamCommander : MonoBehaviour
{
    [SerializeField] private GameObject[] healFountains;
    [SerializeField] private List<FountainController> fountainControllers;
    [SerializeField] private PawnController[] pawns;

    public Camera mainCam;

    private void Awake()
    {
        pawns = FindObjectsOfType<PawnController>();
    }

    void Start()
    {
        healFountains = GameObject.FindGameObjectsWithTag("Healer");
        foreach (GameObject fountain in healFountains)
        {
            FountainController tempFountain = fountain.GetComponent<FountainController>();
            if (tempFountain != null)
                fountainControllers.Add(tempFountain);            
        }
    }

    public Vector3 FindClosestHealFountain(Vector3 targetPosition, int team)
    {
        Vector3 returnPosition = Vector3.zero;
        float distance = 9999999999f;

        foreach (FountainController fountain in fountainControllers.Where(f => f.team == team))
        {
            if (Vector3.Distance(targetPosition, fountain.transform.position) < distance)
            {
                returnPosition = fountain.transform.position;
                distance = Vector3.Distance(targetPosition, returnPosition);
            }
        }

        if (returnPosition == Vector3.zero)
            Debug.Log("FindClosestHealFountain defaulted to vec3.zero, get some sprinklers.");

        return returnPosition;
    }

    public PawnController FindClosestEnemy(Vector3 shooterPosition, int shooterTeam)
    {
        float minDistance = 99999999f;
        PawnController tempEnemy;
        if (pawns[0] != null)
            tempEnemy = pawns[0];
        else
            tempEnemy = new PawnController();

        foreach (PawnController pawn in pawns.Where(p => p != null && p.team != shooterTeam))
        {
            float tempDistance = Vector3.Distance(shooterPosition, pawn.transform.position);
            if (tempDistance < minDistance)
            {
                minDistance = tempDistance;
                tempEnemy = pawn;
            }
        }

        return tempEnemy;
    }
}
