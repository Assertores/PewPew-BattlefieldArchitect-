using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PawnController : MonoBehaviour
{
    private NavMeshAgent navMeshAgent;

    [SerializeField] public int team;
    [SerializeField] private bool isAttacking = false;

    [SerializeField] private float health = 100;
    [SerializeField] private float maxHealth = 100;

    [SerializeField] private float attackDistance = 5f;
    [SerializeField] private float attackDamage = 2f;


    private TeamCommander teamCommander;

    //ai temps
    Vector3 closestFountain;
    PawnController targetEnemy;

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        teamCommander = FindObjectOfType<TeamCommander>();
        closestFountain = teamCommander.FindClosestHealFountain(transform.position, team);
        targetEnemy = teamCommander.FindClosestEnemy(transform.position, team);

        StartCoroutine(ChooseLoop());
    }

    IEnumerator ChooseLoop()
    {
        while (enabled)
        {
            yield return new WaitForSeconds(1f);
            ChooseNextAction();
        }
    }

    #region Evaluating Things
    private void ChooseNextAction()
    {
        closestFountain = teamCommander.FindClosestHealFountain(transform.position, team);
        targetEnemy = teamCommander.FindClosestEnemy(transform.position, team);

        if (health < 50f)
        //if (EvaluateAttack() < EvaluateHeal())
        {
            GoToHeal(closestFountain);
        }
        else
        {
            GoToAttack(targetEnemy.transform.position);
        }
    }

    //ATTACK
    private float EvaluateAttack()
    {
        targetEnemy = teamCommander.FindClosestEnemy(transform.position, team);
        return 1;
    }

    //HEAL
    private float EvaluateHeal()
    {
        //FountainController FindClosestHealFountain
        closestFountain = teamCommander.FindClosestHealFountain(transform.position, team);
        float fountainDistance = Vector3.Distance(transform.position, closestFountain);
        return 1;
    }
    #endregion

    #region Doing Things
    private void GoToAttack(Vector3 target)
    {
        isAttacking = true;
        navMeshAgent.SetDestination(targetEnemy.transform.position);
        if (Vector3.Distance(transform.position, targetEnemy.transform.position) < attackDistance)
            targetEnemy.TakeDamage(attackDamage);
    }

    private void GoToHeal(Vector3 target)
    {
        isAttacking = false;
        navMeshAgent.SetDestination(target);
    }

    public void Heal(float amount)
    {
        health = Mathf.Min(maxHealth, health + amount);
    }

    public void TakeDamage(float amount)
    {
        if (0f < amount)
        {
            health -= amount;
            if (health <= 0f)
                Destroy(gameObject);
        }
    }
    #endregion

    private void SetTeamColour()
    {
        //if team == 0 make yellow, otherwise make wurstcolor
    }

    private void OnDrawGizmos()
    {
        if (isAttacking)
        {
            if (targetEnemy != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, targetEnemy.transform.position);
            }
        }
        else
        {
            if (closestFountain != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, closestFountain);
            }
        }
    }
}
