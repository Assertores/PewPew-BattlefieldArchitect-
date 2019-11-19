using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavSurfaceBaker : MonoBehaviour
{
	[HideInInspector] public const int _navMask = 127;
	public List<NavMeshSurface> _navMeshSurfaces = new List<NavMeshSurface>();

    // Start is called before the first frame update
    void Start()
    {
		foreach(NavMeshSurface surface in _navMeshSurfaces)
		{
            surface.BuildNavMesh();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
