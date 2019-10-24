using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JobCenter : MonoBehaviour
{
    public static JobCenter instance;

    public static List<Blueprint>[] blueprints = new List<Blueprint>[10];
    public static List<ResourceDepot>[] resourceDepots = new List<ResourceDepot>[10];

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        for (int i = 0; i < blueprints.Length; i++)
        {
            if (blueprints[i] == null)
                blueprints[i] = new List<Blueprint>();
            if (resourceDepots[i] == null)
                resourceDepots[i] = new List<ResourceDepot>();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }
}
