using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace PPBA
{
	[RequireComponent(typeof(NavMeshSurface))]
	public class NavAreaRegistration : MonoBehaviour
	{
		[SerializeField] private NavMeshSurface _navSurface;

		void Start()
		{

		}

		void Update()
		{

		}

		private void OnValidate()
		{
			_navSurface = GetComponent<NavMeshSurface>();
		}

		private void OnEnable()
		{
			NavSurfaceBaker._surfaces.Add(_navSurface);
		}

		private void OnDisable()
		{
			if(NavSurfaceBaker._surfaces.Contains(_navSurface))
				NavSurfaceBaker._surfaces.Remove(_navSurface);
		}
	}
}