using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Behavior_Shoot : Behavior
{
    public static Behavior_Shoot instance;
    
    public Dictionary<Pawn, Pawn> targetDictionary;
    

    void Awake()//my own singleton pattern, the Singleton.cs doesn't work here as I need multiple behaviors.
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
        throw new System.NotImplementedException();
    }

    public override float FindBestTarget(Pawn pawn)
    {
        throw new System.NotImplementedException();
    }

    protected override float PawnAxisInputs(Pawn pawn, string name)
    {
        throw new System.NotImplementedException();
    }

    protected float TargetAxisInputs(Pawn pawn, string name)
    {
        return 1;
    }
}
