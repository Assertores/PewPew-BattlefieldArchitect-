using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FountainController : MonoBehaviour
{
    public int team;
    [SerializeField] private List<PawnController> closePawns;
    [SerializeField] private float healAmount = 1f;

    private void Start()
    {
        StartCoroutine(HealSpray());
    }

    IEnumerator HealSpray()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            SprayHeal();
        }
    }

    private void SprayHeal()
    {
        foreach (PawnController pawn in closePawns.Where(p => p != null && p.team == team))
        {
            pawn.Heal(healAmount);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Pawn")
        {
            PawnController tempPawn = other.GetComponent<PawnController>();
            if (tempPawn != null)
                closePawns.Add(tempPawn);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Pawn")
        {
            PawnController tempPawn = other.GetComponent<PawnController>();
            if (tempPawn != null && closePawns.Contains(tempPawn))
                closePawns.Remove(tempPawn);
        }
    }
}
