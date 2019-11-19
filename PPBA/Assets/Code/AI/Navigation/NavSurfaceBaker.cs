using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace PPBA
{
	public class NavSurfaceBaker : MonoBehaviour
	{
		[HideInInspector] public const int _navMask = 127;
		public static List<NavMeshSurface> _surfaces = new List<NavMeshSurface>();
		public static bool _isSurfacesDirty = false;//let spawning obstacles set this to true
		public static bool _isPathsDirty = false;//global variable to let pawns know the NavMesh has changed

		// Start is called before the first frame update
		void Start()
		{

		}

		// Update is called once per frame
		void Update()
		{

		}

		private void BuildNavMesh()
		{
			foreach(NavMeshSurface surface in _surfaces)
			{
				surface.BuildNavMesh();
			}
		}

		public void ReadyNavMesh(int tick = 0)
		{
			if(_isSurfacesDirty)
			{
				BuildNavMesh();
				_isPathsDirty = true;
			}

			_isSurfacesDirty = false;
		}

		public void ReadyPathsFlag(int tick = 0)
		{
			_isPathsDirty = false;
		}

		#region Initialisation
		private void OnEnable()
		{
			TickHandler.s_DoInput += ReadyPathsFlag;//this is not input related, but has to happen before EarlyCalc and after DoTick
			TickHandler.s_EarlyCalc += ReadyNavMesh;
		}

		private void OnDisable()
		{
			TickHandler.s_DoInput -= ReadyPathsFlag;
			TickHandler.s_EarlyCalc -= ReadyNavMesh;
		}
		#endregion
	}
}
