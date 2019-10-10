using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Pawn : MonoBehaviour
{
    public enum Behaviors { GoAnywhere, Shoot, Heal };


    [SerializeField] public int team;
    private int enemyTeam;
    [SerializeField] private bool isAttacking = false;

    [SerializeField] private float health = 100;
    [SerializeField] private float maxHealth = 100;

    [SerializeField] private float attackDistance = 5f;
    [SerializeField] private float attackDamage = 2f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public abstract float Evaluate();
    public abstract void Execute(Behavior behavior);
}
