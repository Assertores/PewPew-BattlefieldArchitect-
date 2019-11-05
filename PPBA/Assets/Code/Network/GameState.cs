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

			public gsc(int id)
			{
				_id = id;
			}
		}

		public class type : gsc
		{
			public byte _type;
			public byte _team;

			public type(int _id, byte type, byte team) : base(_id)
			{
				_type = type;
				_team = team;
			}
		}

		public class arg : gsc
		{
			public Arguments _arguments;

			public arg(int _id, Arguments arguments) : base(_id)
			{
				_arguments = arguments;
			}
		}

		public class transform : gsc
		{
			public Vector3 _position;
			public float _angle;

			public transform(int _id, Vector3 position, float angle) : base(_id)
			{
				_position = position;
				_angle = angle;
			}
		}

		public class ammo : gsc
		{
			public int _bullets;
			//public int _grenades;

			public ammo(int _id, int bullets) : base(_id)
			{
				_bullets = bullets;
			}
		}

		public class resource : gsc
		{
			public int _resources;

			public resource(int _id, int resources) : base(_id)
			{
				_resources = resources;
			}
		}
		
		public class health : gsc
		{
			public float _health;
			public float _morale;

			public health(int _id, float health, float morale) : base(_id)
			{
				_health = health;
				_morale = morale;
			}
		}

		public class work : gsc
		{
			public int _work;

			public work(int _id, int work) : base(_id)
			{
				_work = work;
			}
		}

		public class behavior : gsc
		{
			public Behavior _behavior;

			public behavior(int _id, PPBA.Behavior behavior) : base(_id)
			{
				_behavior = behavior;
			}
		}

		public class behavior_id : behavior
		{
			public int _target;

			public behavior_id(int _id, PPBA.Behavior _behavior, int target) : base(_id, _behavior)
			{
				_target = target;
			}
		}

		public class behavior_vec : behavior
		{
			public Vector3 _targetVec;

			public behavior_vec(int _id, PPBA.Behavior _behavior, Vector3 target) : base(_id, _behavior)
			{
				_targetVec = target;
			}
		}

		public class path : gsc
		{
			//public List<Vector3> _path;
			public Vector3[] _path;

			public path(int _id, Vector3[] path) : base(_id)
			{
				_path = path;
			}
		}

		public class map : gsc
		{
			public BitField2D _mask;
			List<float> _values;

			public map(int _id, BitField2D mask, List<float> values) : base(_id)
			{
				_mask = mask;
				_values = values;
			}
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
		public List<GSC.work> _works = new List<GSC.work>();
		public List<GSC.behavior> _behaviors = new List<GSC.behavior>();
		public List<GSC.path> _paths = new List<GSC.path>();
		public List<GSC.map> _maps = new List<GSC.map>();
		public List<int> _denyedInputIDs = new List<int>();

	}
}