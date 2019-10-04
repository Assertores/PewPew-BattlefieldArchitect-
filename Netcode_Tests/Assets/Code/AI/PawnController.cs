using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class PawnController : MonoBehaviour
{
    private NavMeshAgent navMeshAgent;

    [SerializeField] public int team;
    private int enemyTeam;
    [SerializeField] private bool isAttacking = false;

    [SerializeField] private float health = 100;
    [SerializeField] private float maxHealth = 100;

    [SerializeField] private float attackDistance = 5f;
    [SerializeField] private float attackDamage = 2f;


    private TeamCommander teamCommander;

    //ai temps
    Vector3 closestFountain;
    PawnController targetEnemy;

    Slider healthSlider;
    GameObject sliderObject;
    Camera mainCam;

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        teamCommander = FindObjectOfType<TeamCommander>();
        closestFountain = teamCommander.FindClosestHealFountain(transform.position, team);
        targetEnemy = teamCommander.FindClosestEnemy(transform.position, team);
        healthSlider = GetComponentInChildren<Slider>();
        sliderObject = healthSlider.gameObject;
        mainCam = teamCommander.mainCam;

        StartCoroutine(ChooseLoop());
        StartCoroutine(DrinkFromFountainLoop());

        if (team == 0)
            enemyTeam = 1;
        else
            enemyTeam = 0;
    }

    void Update()
    {
        healthSlider.value = Mathf.Clamp(health / maxHealth, 0f, 1f);
        sliderObject.transform.LookAt(mainCam.transform);
    }

    IEnumerator ChooseLoop()
    {
        while (enabled)
        {
            yield return new WaitForSeconds(0.25f);
            ChooseNextAction();
        }
    }

    IEnumerator DrinkFromFountainLoop()
    {
        while (enabled)
        {
            yield return new WaitForSeconds(0.25f);
            DrinkFromFountain();
        }
    }

    #region Evaluating Things
    private void ChooseNextAction()
    {
        closestFountain = teamCommander.FindClosestHealFountain(transform.position, team);
        targetEnemy = teamCommander.FindClosestEnemy(transform.position, team);

        if (EvaluateAttack() < EvaluateHeal())
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
        float enemyDistance = Mathf.Clamp(Vector3.Distance(transform.position, targetEnemy.transform.position), 0f, 30f);
        float enemyDistanceFactor = Mathf.Clamp(1f - 0.3f * Mathf.Clamp(enemyDistance / 30f, 0f, 1f), 0f, 1f);
        float enemyHealthFactor = Mathf.Clamp(1f - 0.15f * Mathf.Clamp(targetEnemy.health / targetEnemy.maxHealth, 0f, 1f), 0f, 1f);
        float ownHealthFactor = Mathf.Clamp(0.5f + 0.5f * Mathf.Clamp(health / maxHealth, 0f, 1f), 0f, 1f);
        float enemyFountainDistance = Vector3.Distance(transform.position, teamCommander.FindClosestHealFountain(transform.position, enemyTeam));
        float enemyFountainFactor = Mathf.Clamp(enemyFountainDistance / 5f, 0f, 1f);
        return enemyDistanceFactor * enemyHealthFactor * ownHealthFactor * enemyFountainDistance;
    }

    //HEAL
    private float EvaluateHeal()
    {
        closestFountain = teamCommander.FindClosestHealFountain(transform.position, team);
        float fountainDistance = Vector3.Distance(transform.position, closestFountain);
        float fountainDistanceFactor = Mathf.Clamp(1f - 0.1f * Mathf.Clamp((fountainDistance / 30), 0f, 1f), 0f, 1f);
        float ownHealthFactor = Mathf.Clamp(1f - 1f * Mathf.Clamp(health / maxHealth, 0f, 1f), 0f, 1f);
        return fountainDistanceFactor * ownHealthFactor;
    }
    #endregion

    #region Doing Things
    private void GoToAttack(Vector3 target)
    {
        isAttacking = true;
        if (navMeshAgent.enabled)
            navMeshAgent.SetDestination(targetEnemy.transform.position);
        if (Vector3.Distance(transform.position, targetEnemy.transform.position) < attackDistance)
            targetEnemy.TakeDamage(attackDamage);
    }

    private void GoToHeal(Vector3 target)
    {
        isAttacking = false;
        if (navMeshAgent.enabled)
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
            {
                transform.position = Vector3.forward * 1000f;
                navMeshAgent.enabled = false;
                Destroy(gameObject, 1f);
            }
        }
    }

    private void DrinkFromFountain()
    {
        closestFountain = teamCommander.FindClosestHealFountain(transform.position, team);
        float fountainDistance = Vector3.Distance(transform.position, closestFountain);

        if (fountainDistance < 3f)
        {
            health = Mathf.Clamp(health + 5f, 0f, maxHealth);
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
