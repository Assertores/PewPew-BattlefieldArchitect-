using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(SphereCollider))]
public abstract class Pawn : MonoBehaviour
{
    //Behaviors
    protected enum Behaviors { GoAnywhere, Shoot, Heal };
    [SerializeField] protected Behaviors[] e_behaviors;
    protected Behavior[] behaviors;
    [SerializeField] [Tooltip("Displays last calculated behavior-scores.\nNo reason to change these.")] protected float[] behavior_scores;

    //public
    [SerializeField] public int team;

    //protected
    [SerializeField] protected bool isAttacking = false;

    [SerializeField] public float health = 100;
    [SerializeField] public float maxHealth = 100;

    [SerializeField] public float attackDistance = 5f;
    [SerializeField] public float attackDamage = 2f;

    //Components
    //[HideInInspector]
    public NavMeshAgent navMeshAgent;
    private SphereCollider sphereCollider;

    private object target;

    //target lists
    // pawns, covers, depots, bringjobs, buildjobs, deconstructjobs
    public List<Pawn> closePawns;
    public List<Cover> closeCover;

    // Start is called before the first frame update
    public void Start()
    {
        InitiateBehaviors();
        navMeshAgent = GetComponent<NavMeshAgent>();
        sphereCollider = GetComponent<SphereCollider>();
        StartCoroutine(DoTick());
    }

    // Update is called once per frame
    public void Update()
    {

    }

    protected void Evaluate()   //uses behavior-scores to evaluate behaviors
    {
        for (int i = 0; i < behaviors.Length; i++)
        {
            behavior_scores[i] = behaviors[i].Calculate(this);
        }
    }

    protected void Execute()   //calls the execution on the most appropriate behavior
    {
        int best_behavior = 0;
        float best_score = 0;

        for (int i = 0; i < behavior_scores.Length; i++)//determines best behavior
        {
            if (best_score < behavior_scores[i])
            {
                best_behavior = i;
                best_score = behavior_scores[i];
            }
        }

        behaviors[best_behavior].Execute(this);
    }

    #region Initialisation
    protected void InitiateBehaviors()  //reads the behaviors from the enum-array
    {
        behaviors = new Behavior[e_behaviors.Length];
        behavior_scores = new float[e_behaviors.Length];

        for (int i = 0; i < e_behaviors.Length; i++)
        {
            behaviors[i] = GetBehavior(e_behaviors[i]);
        }
    }

    protected Behavior GetBehavior(Behaviors e_behaviors)   //switch used for initialising behaviour-array
    {
        switch (e_behaviors)
        {
            case Behaviors.GoAnywhere:
                return Behavior_GoAnywhere.instance;
            case Behaviors.Shoot:
                return Behavior_Shoot.instance;
            case Behaviors.Heal:
                return Behavior_GoAnywhere.instance;
            default:
                Debug.LogWarning("GetBehavior switch defaulted. Couldn't get the desired behavior.");
                return Behavior_GoAnywhere.instance;
        }
    }
    #endregion

    #region FakeTick
    protected IEnumerator DoTick()
    {
        while (enabled)
        {
            yield return new WaitForSeconds(0.2f);
            Evaluate();
            Execute();
        }
    }
    #endregion

    #region Physics
    private void OnTriggerEnter(Collider other)
    {
        //Add relevant objects to closeLists
        if (other.tag == "Pawn")
        {
            Pawn temp = other.gameObject.GetComponent<Pawn>();
            if (temp)
                closePawns.Add(temp);
        }
        
        if (other.tag == "Cover")
        {
            Cover temp = other.gameObject.GetComponent<Cover>();
            if (temp)
                closeCover.Add(temp);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //Remove objects from closeLists
        if (other.tag == "Pawn")
        {
            Pawn temp = other.gameObject.GetComponent<Pawn>();
            if (temp && closePawns.Contains(temp))
                closePawns.Remove(temp);
        }

        if (other.tag == "Cover")
        {
            Cover temp = other.gameObject.GetComponent<Cover>();
            if (temp && closeCover.Contains(temp))
                closeCover.Remove(temp);
        }
    }
    #endregion

    #region Gizmos
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, navMeshAgent.destination);
    }
    #endregion
}
