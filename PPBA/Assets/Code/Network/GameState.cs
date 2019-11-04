using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	[System.Flags]
	public enum Arguments : byte {
		NON = 0,
		ENABLED = 1,//2^0
		TRIGGERBEHAVIOUR = 2,//2^1
	};

	namespace GSC //Game State Component
	{
		public class gsc
		{
			public int _id;
		}

		public class type : gsc
		{
			public byte _type;
			public byte _team;
		}

		public class arg : gsc
		{
			public Arguments _arguments;
		}

		public class transform : gsc
		{
			public Vector3 _position;
			public float _angle;
		}

		public class ammo : gsc
		{
			public int _bullets;
			//public int _grenades;
		}

		public class resource : gsc
		{
			public int _resources;
		}
		
		public class health : gsc
		{
			public float _health;
			public float _morale;
		}

		public class behavior
		{
			public Behavior _behavior;
			public int _target;
		}

		public class path : gsc
		{
			public List<Vector3> _path;
		}

		public class map : gsc
		{
			public BitField2D _mask;
			List<float> _values;
		}
	}
	public class GameState
	{
		public List<GSC.type> _types = new List<GSC.type>();
		public List<GSC.arg> _args = new List<GSC.arg>();
		public List<GSC.transform> _transforms = new List<GSC.transform>();
		public List<GSC.ammo> _ammos = new List<GSC.ammo>();
		public List<GSC.resource> _resources = new List<GSC.resource>();
		public List<GSC.health> _healths = new List<GSC.health>();
		public List<GSC.behavior> _behaviors = new List<GSC.behavior>();
		public List<GSC.path> _paths = new List<GSC.path>();
		public List<GSC.map> _maps = new List<GSC.map>();
		public List<int> _denyedInputIDs = new List<int>();

	}
}