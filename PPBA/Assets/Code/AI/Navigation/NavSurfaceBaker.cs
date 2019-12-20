using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace PPBA
{
	public class NavSurfaceBaker : Singleton<NavSurfaceBaker>
	{
		[HideInInspector] public const int _navMask = 127;
		public static List<NavMeshSurface> _surfaces = new List<NavMeshSurface>();
		public static bool _isSurfacesDirty = false;//let spawning obstacles set this to true
		public static bool _isPathsDirty = false;//global variable to let pawns know the NavMesh has changed, they will recalculate before moving

		/*
		public static List<NavMeshModifier> _navMods = new List<NavMeshModifier>();
		public static List<NavMeshModifierVolume> _navModVolumes = new List<NavMeshModifierVolume>();
		public static NavMeshSurface _navSurface;
		*/

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
			_isSurfacesDirty = true;
		}

		#region Initialisation
		private void OnEnable()
		{
#if UNITY_SERVER
			TickHandler.s_SetUp += ReadyPathsFlag;//this has to happen before DoInput and after DoTick
			TickHandler.s_EarlyCalc += ReadyNavMesh;
#endif
		}

		private void OnDisable()
		{
#if UNITY_SERVER
			//TickHandler.s_Setup -= ReadyPathsFlag;
			TickHandler.s_EarlyCalc -= ReadyNavMesh;
#endif
		}
		#endregion
	}
}
