using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace PPBA
{
	[RequireComponent(typeof(NavMeshAgent))]
	[RequireComponent(typeof(LineRenderer))]
	public class NavTestPawn : MonoBehaviour
	{
		[SerializeField] private GameObject objectToFollow;
		private NavMeshAgent _navMeshAgent;
		private LineRenderer _lineRenderer;

		void Start()
		{
			_navMeshAgent = GetComponent<NavMeshAgent>();
			_lineRenderer = GetComponent<LineRenderer>();
		}

		void Update()
		{
			if(objectToFollow != null && _navMeshAgent != null)
				_navMeshAgent.SetDestination(objectToFollow.transform.position);

			if(_lineRenderer)
			{
				ShowNavPath();
			}
		}

		public void ShowNavPath()
		{
			_lineRenderer.positionCount = _navMeshAgent.path.corners.Length;
			_lineRenderer.SetPositions(_navMeshAgent.path.corners);
		}
	}
}
