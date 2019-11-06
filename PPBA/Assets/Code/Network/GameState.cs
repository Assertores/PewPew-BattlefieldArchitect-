using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.ComponentModel;

namespace PPBA
{
	[System.Flags]
	public enum Arguments : byte
	{
		NON = 0,
		ENABLED = 1,//2^0
		TRIGGERBEHAVIOUR = 2,//2^1
	};

	namespace GSC //Game State Component
	{
		enum DataType : byte { NON, TYPE, ARGUMENT, TRANSFORM, AMMO, RESOURCE, HEALTH, BEHAVIOUR, PATH, MAP, INPUTS };

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
			public float _angle; //in degrees
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
			public float _morale;//used for _score by depots/blueprints
		}

		public class work : gsc
		{
			public int _work;
		}

		public class behavior : gsc
		{
			public Behaviors _behavior;
			public int _target;
		}

		public class path : gsc
		{
			//public List<Vector3> _path;
			public Vector3[] _path;
		}

		public class map : gsc
		{
			public BitField2D _mask;
			public List<float> _values;
		}
	}
	public class GameState
	{
		public int _refTick { get; private set; } = -1;
		public bool _isLerped { get; private set; }
		public bool _isDelta { get; private set; }
		//public byte _messageCount;
		public BitField2D _receivedMessages;
		private List<byte[]> _messageHolder = null;

		public List<GSC.type> _types = new List<GSC.type>();
		public List<GSC.arg> _args = new List<GSC.arg>();
		public List<GSC.transform> _transforms = new List<GSC.transform>();
		public List<GSC.ammo> _ammos = new List<GSC.ammo>();
		public List<GSC.resource> _resources = new List<GSC.resource>();
		public List<GSC.health> _healths = new List<GSC.health>();
		public List<GSC.work> _works = new List<GSC.work>();
		public List<GSC.behavior> _behaviors = new List<GSC.behavior>();
		public List<GSC.path> _paths = new List<GSC.path>();
		//public List<GSC.map> _maps = new List<GSC.map>();
		//public List<int> _denyedInputIDs = new List<int>();

		public List<byte[]> Encrypt(int maxPackageSize)
		{
			if(_messageHolder != null)
				return _messageHolder;

			List<byte[]> value = new List<byte[]>();
			List<byte> msg = new List<byte>();

			if(_types.Count > 0)
			{
				msg.Clear();
				msg.Add((byte)GSC.DataType.TYPE);
				msg.AddRange(BitConverter.GetBytes(_types.Count));
				foreach(var it in _types)
				{
					msg.AddRange(BitConverter.GetBytes(it._id));
					msg.Add(it._type);
					msg.Add(it._team);
				}

				HandlePackageSize(maxPackageSize, value, msg.ToArray());
			}
			if(_args.Count > 0)
			{
				msg.Clear();
				msg.Add((byte)GSC.DataType.ARGUMENT);
				msg.AddRange(BitConverter.GetBytes(_args.Count));
				foreach(var it in _args)
				{
					msg.AddRange(BitConverter.GetBytes(it._id));
					msg.Add((byte)it._arguments);
				}

				HandlePackageSize(maxPackageSize, value, msg.ToArray());
			}
			if(_transforms.Count > 0)
			{
				msg.Clear();
				msg.Add((byte)GSC.DataType.TRANSFORM);
				msg.AddRange(BitConverter.GetBytes(_transforms.Count));
				foreach(var it in _transforms)
				{
					msg.AddRange(BitConverter.GetBytes(it._id));
					msg.AddRange(BitConverter.GetBytes(it._position.x));
					msg.AddRange(BitConverter.GetBytes(it._position.y));
					msg.AddRange(BitConverter.GetBytes(it._position.z));
					msg.AddRange(BitConverter.GetBytes(it._angle));
				}

				HandlePackageSize(maxPackageSize, value, msg.ToArray());
			}
			if(_ammos.Count > 0)
			{
				msg.Clear();
				msg.Add((byte)GSC.DataType.AMMO);
				msg.AddRange(BitConverter.GetBytes(_ammos.Count));
				foreach(var it in _ammos)
				{
					msg.AddRange(BitConverter.GetBytes(it._id));
					msg.AddRange(BitConverter.GetBytes(it._bullets));
					//msg.AddRange(BitConverter.GetBytes(it._grenades));
				}

				HandlePackageSize(maxPackageSize, value, msg.ToArray());
			}
			if(_resources.Count > 0)
			{
				msg.Clear();
				msg.Add((byte)GSC.DataType.RESOURCE);
				msg.AddRange(BitConverter.GetBytes(_resources.Count));
				foreach(var it in _resources)
				{
					msg.AddRange(BitConverter.GetBytes(it._id));
					msg.AddRange(BitConverter.GetBytes(it._resources));
				}

				HandlePackageSize(maxPackageSize, value, msg.ToArray());
			}
			if(_healths.Count > 0)
			{
				msg.Clear();
				msg.Add((byte)GSC.DataType.HEALTH);
				msg.AddRange(BitConverter.GetBytes(_healths.Count));
				foreach(var it in _healths)
				{
					msg.AddRange(BitConverter.GetBytes(it._id));
					msg.AddRange(BitConverter.GetBytes(it._health));
					msg.AddRange(BitConverter.GetBytes(it._morale));
				}

				HandlePackageSize(maxPackageSize, value, msg.ToArray());
			}
			if(_behaviors.Count > 0)
			{
				msg.Clear();
				msg.Add((byte)GSC.DataType.BEHAVIOUR);
				msg.AddRange(BitConverter.GetBytes(_behaviors.Count));
				foreach(var it in _behaviors)
				{
					msg.AddRange(BitConverter.GetBytes(it._id));
					msg.Add((byte)it._behavior);
					msg.AddRange(BitConverter.GetBytes(it._target));
				}

				HandlePackageSize(maxPackageSize, value, msg.ToArray());
			}
			if(_paths.Count > 0) //byte type, int length, [struct elements, int pathLength, [Vec3 positions]]
			{
				msg.Clear();
				msg.Add((byte)GSC.DataType.PATH);
				msg.AddRange(BitConverter.GetBytes(_paths.Count));
				foreach(var it in _paths)
				{
					msg.AddRange(BitConverter.GetBytes(it._id));
					msg.AddRange(BitConverter.GetBytes(it._path.Length));
					for(int i = 0; i < it._path.Length; i++)
					{
						msg.AddRange(BitConverter.GetBytes(it._path[i].x));
						msg.AddRange(BitConverter.GetBytes(it._path[i].y));
						msg.AddRange(BitConverter.GetBytes(it._path[i].z));
					}
				}

				HandlePackageSize(maxPackageSize, value, msg.ToArray());
			}
			/*if(_maps.Count > 0)
			{
				foreach(var it in _maps)
				{
					msg.Clear();
					msg.Add((byte)GSC.DataType.MAP);
					msg.AddRange(BitConverter.GetBytes(it._id));//overloads the count bytes in compareson to all other types
					msg.AddRange(it._mask.ToArray());
					for(int i = 0; i < it._values.Count; i++)
					{
						msg.AddRange(BitConverter.GetBytes(it._values[i]));
					}

					HandlePackageSize(maxPackageSize, value, msg.ToArray());
				}
			}*/
			/*if(_denyedInputIDs.Count > 0)
			{
				msg.Clear();
				msg.Add((byte)GSC.DataType.INPUTS);
				msg.AddRange(BitConverter.GetBytes(_denyedInputIDs.Count));
				foreach(var it in _denyedInputIDs)
				{
					msg.AddRange(BitConverter.GetBytes(it));
				}

				HandlePackageSize(maxPackageSize, value, msg.ToArray());
			}*/

			_messageHolder = value;
			return value;
		}

		public void Decrypt(byte[] msg, int offset)
		{
			int count;
			while(offset < msg.Length)
			{
				count = 0;
				count = BitConverter.ToInt32(msg, offset + 1);
				offset += sizeof(int) + 1;
				switch((GSC.DataType)msg[offset - 1 - sizeof(int)])
				{
					case GSC.DataType.NON:
						goto default;
					case GSC.DataType.TYPE:
					{
						_types = new List<GSC.type>(count);
						for(int i = 0; i < count; i++)
						{
							GSC.type value = new GSC.type();
							value._id = BitConverter.ToInt32(msg, offset);
							offset += sizeof(int);

							value._type = msg[offset];
							offset++;

							value._team = msg[offset];
							offset++;

							_types.Add(value);
						}
						break;
					}
					case GSC.DataType.ARGUMENT:
					{
						_args = new List<GSC.arg>(count);
						for(int i = 0; i < count; i++)
						{
							GSC.arg value = new GSC.arg();

							value._id = BitConverter.ToInt32(msg, offset);
							offset += sizeof(int);

							value._arguments = (Arguments)msg[offset];
							offset++;

							_args.Add(value);
						}
						break;
					}
					case GSC.DataType.TRANSFORM:
					{
						_transforms = new List<GSC.transform>(count);
						for(int i = 0; i < count; i++)
						{
							GSC.transform value = new GSC.transform();

							value._id = BitConverter.ToInt32(msg, offset);
							offset += sizeof(int);

							value._position = new Vector3(
															BitConverter.ToSingle(msg, offset),
															BitConverter.ToSingle(msg, offset + sizeof(float)),
															BitConverter.ToSingle(msg, offset + 2 * sizeof(float))
														 );
							offset += 3 * sizeof(float);

							value._angle = BitConverter.ToSingle(msg, offset);
							offset += sizeof(float);

							_transforms.Add(value);
						}
						break;
					}
					case GSC.DataType.AMMO:
					{
						_ammos = new List<GSC.ammo>(count);
						for(int i = 0; i < count; i++)
						{
							GSC.ammo value = new GSC.ammo();

							value._id = BitConverter.ToInt32(msg, offset);
							offset += sizeof(int);

							value._bullets = BitConverter.ToInt32(msg, offset);
							offset += sizeof(int);

							/*value._grenades = BitConverter.ToInt32(msg, offset);
							offset += sizeof(int);*/

							_ammos.Add(value);
						}
						break;
					}
					case GSC.DataType.RESOURCE:
					{
						_resources = new List<GSC.resource>(count);
						for(int i = 0; i < count; i++)
						{
							GSC.resource value = new GSC.resource();

							value._id = BitConverter.ToInt32(msg, offset);
							offset += sizeof(int);

							value._resources = BitConverter.ToInt32(msg, offset);
							offset += sizeof(int);

							_resources.Add(value);
						}
						break;
					}
					case GSC.DataType.HEALTH:
					{
						_healths = new List<GSC.health>(count);
						for(int i = 0; i < count; i++)
						{
							GSC.health value = new GSC.health();

							value._id = BitConverter.ToInt32(msg, offset);
							offset += sizeof(int);

							value._health = BitConverter.ToInt32(msg, offset);
							offset += sizeof(int);

							value._morale = BitConverter.ToSingle(msg, offset);
							offset += sizeof(float);

							_healths.Add(value);
						}
						break;
					}
					case GSC.DataType.BEHAVIOUR:
					{
						_behaviors = new List<GSC.behavior>(count);
						for(int i = 0; i < count; i++)
						{
							GSC.behavior value = new GSC.behavior();

							value._id = BitConverter.ToInt32(msg, offset);
							offset += sizeof(int);

							value._behavior = (Behaviors)msg[offset];
							offset++;

							value._target = BitConverter.ToInt32(msg, offset);
							offset += sizeof(int);

							_behaviors.Add(value);
						}
						break;
					}
					case GSC.DataType.PATH:
					{
						_paths = new List<GSC.path>(count);
						for(int i = 0; i < count; i++)
						{
							GSC.path value = new GSC.path();

							value._id = BitConverter.ToInt32(msg, offset);
							offset += sizeof(int);

							int size = BitConverter.ToInt32(msg, offset);
							offset += sizeof(int);

							value._path = new Vector3[size];
							for(int j = 0; j < size; j++)
							{
								value._path[j] = (new Vector3(
															BitConverter.ToSingle(msg, offset),
															BitConverter.ToSingle(msg, offset + sizeof(float)),
															BitConverter.ToSingle(msg, offset + 2 * sizeof(float))
															));
								offset += 3 * sizeof(float);
							}

							_paths.Add(value);
						}
						break;
					}
					/*case GSC.DataType.MAP://TODO: Renes HeatMap
					{
						GSC.map value = new GSC.map();
						value._id = count;//value overload

						byte[] mask = new byte[0];//TODO: interface to Renes Compute Shader HeatMaps
						Buffer.BlockCopy(msg, offset, mask, 0, mask.Length);
						value._mask = new BitField2D(0, 0, mask);//TODO: interface to Renes Compute Shader HeatMaps
						offset += mask.Length;

						int size = value._mask.GetActiveBits().Length;
						value._values = new List<float>(size);
						for(int j = 0; j < size; j++)
						{
							value._values.Add(BitConverter.ToSingle(msg, offset));
							offset += sizeof(float);
						}

						_maps.Add(value);
						break;
					}*/
					/*case GSC.DataType.INPUTS:
					{
						_denyedInputIDs = new List<int>(count);
						for(int i = 0; i < count; i++)
						{
							_denyedInputIDs.Add(BitConverter.ToInt32(msg, offset));
							offset += sizeof(int);
						}
						break;
					}*/
					default:
						throw new InvalidEnumArgumentException();
				}
			}
		}

		public bool CreateDelta(GameState reference, int refTick)
		{
			_refTick = refTick;

			foreach(var it in reference._types)
			{
				GSC.type element = _types.Find(x => x._id == it._id);

				if(it._type != element._type)
					continue;
				if(it._team != element._team)
					continue;

				_types.Remove(element);
			}
			foreach(var it in reference._args)
			{
				GSC.arg element = _args.Find(x => x._id == it._id);

				if(it._arguments != element._arguments)
					continue;

				_args.Remove(element);
			}
			foreach(var it in reference._transforms)
			{
				GSC.transform element = _transforms.Find(x => x._id == it._id);

				if(it._position != element._position)
					continue;
				if(it._angle != element._angle)
					continue;

				_transforms.Remove(element);
			}
			foreach(var it in reference._ammos)
			{
				GSC.ammo element = _ammos.Find(x => x._id == it._id);

				if(it._bullets != element._bullets)
					continue;
				/*if(it._grenades != element._grenades)
					continue;*/

				_ammos.Remove(element);
			}
			foreach(var it in reference._resources)
			{
				GSC.resource element = _resources.Find(x => x._id == it._id);

				if(it._resources != element._resources)
					continue;

				_resources.Remove(element);
			}
			foreach(var it in reference._healths)
			{
				GSC.health element = _healths.Find(x => x._id == it._id);

				if(it._health != element._health)
					continue;
				if(it._morale != element._morale)
					continue;

				_healths.Remove(element);
			}
			foreach(var it in reference._behaviors)
			{
				GSC.behavior element = _behaviors.Find(x => x._id == it._id);

				if(it._behavior != element._behavior)
					continue;
				if(it._target != element._target)
					continue;

				_behaviors.Remove(element);
			}
			foreach(var it in reference._paths)
			{
				GSC.path element = _paths.Find(x => x._id == it._id);

				if(it._path.Length != element._path.Length)
					continue;

				/*
				{
					int i = 0;
					for(; i < it._path.Count; i++)
					{
						if(it._path[i] != element._path[i])
							break;
					}
					if(i < it._path.Count)
						continue;
				}
				*/

				for(int i = 0; i < it._path.Length; i++)
				{
					if(it._path[i] != element._path[i])
						goto Failed;
				}
				goto Jump;
Failed:
				continue;
Jump:

				_paths.Remove(element);
			}
			/*foreach(var it in reference._maps)
			{
				GSC.map element = _maps.Find(x => x._id == it._id);

				if(it. != element._arguments)
					continue;

				_maps.Remove(element);
			}*/

			return true;
		}

		public bool DismantleDelta(GameState reference, List<int> expactedInputs)
		{
			_messageHolder = null;

			foreach(var it in reference._types)
			{
				if(_types.Exists(x => x._id == it._id))
					continue;

				_types.Add(it);
			}
			foreach(var it in reference._args)
			{
				if(_args.Exists(x => x._id == it._id))
					continue;

				_args.Add(it);
			}
			foreach(var it in reference._transforms)
			{
				if(_transforms.Exists(x => x._id == it._id))
					continue;

				_transforms.Add(it);
			}
			foreach(var it in reference._ammos)
			{
				if(_ammos.Exists(x => x._id == it._id))
					continue;

				_ammos.Add(it);
			}
			foreach(var it in reference._resources)
			{
				if(_resources.Exists(x => x._id == it._id))
					continue;

				_resources.Add(it);
			}
			foreach(var it in reference._healths)
			{
				if(_healths.Exists(x => x._id == it._id))
					continue;

				_healths.Add(it);
			}
			foreach(var it in reference._behaviors)
			{
				if(_behaviors.Exists(x => x._id == it._id))
					continue;

				_behaviors.Add(it);
			}
			foreach(var it in reference._paths)
			{
				if(_paths.Exists(x => x._id == it._id))
					continue;

				_paths.Add(it);
			}

			return true;
		}

		public static GameState Lerp(GameState start, GameState end, int lerpValue)
		{
			GameState value = new GameState();

			foreach(var origin in start._types)
			{
				GSC.type target = end._types.Find(x => x._id == origin._id);
				value._types.Add(new GSC.type
				{
					_id = origin._id,
					_type = (lerpValue < 0.5f) ? origin._type : target._type,
					_team = (lerpValue < 0.5f) ? origin._team : target._team,
				});
			}
			foreach(var origin in start._args)
			{
				GSC.arg target = end._args.Find(x => x._id == origin._id);
				value._args.Add(new GSC.arg
				{
					_id = origin._id,
					_arguments = (lerpValue < 0.5f) ? origin._arguments : target._arguments,
				});
			}
			foreach(var origin in start._transforms)
			{
				GSC.transform target = end._transforms.Find(x => x._id == origin._id);
				value._transforms.Add(new GSC.transform
				{
					_id = origin._id,
					_position = Vector3.Lerp(origin._position, target._position, lerpValue),
					_angle = Mathf.LerpAngle(origin._angle, target._angle, lerpValue),
				});
			}
			foreach(var origin in start._ammos)
			{
				GSC.ammo target = end._ammos.Find(x => x._id == origin._id);
				value._ammos.Add(new GSC.ammo
				{
					_id = origin._id,
					_bullets = (int)Mathf.Lerp(origin._bullets, target._bullets, lerpValue),
					//_grenades = (int)Mathf.Lerp(origin._grenades, target._grenades, lerpValue),
				});
			}
			foreach(var origin in start._resources)
			{
				GSC.resource target = end._resources.Find(x => x._id == origin._id);

				value._resources.Add(new GSC.resource
				{
					_id = origin._id,
					_resources = (int)Mathf.Lerp(origin._resources, target._resources, lerpValue),
				});
			}
			foreach(var origin in start._healths)
			{
				GSC.health target = end._healths.Find(x => x._id == origin._id);
				value._healths.Add(new GSC.health
				{
					_id = origin._id,
					_health = Mathf.Lerp(origin._health, target._health, lerpValue),
					_morale = Mathf.Lerp(origin._morale, target._morale, lerpValue),
				});
			}
			foreach(var origin in start._behaviors)
			{
				GSC.behavior target = end._behaviors.Find(x => x._id == origin._id);
				value._behaviors.Add(new GSC.behavior
				{
					_id = origin._id,
					_behavior = (lerpValue < 0.5f) ? origin._behavior : target._behavior,
					_target = (lerpValue < 0.5f) ? origin._target : target._target,
				});
			}
			foreach(var origin in start._paths)
			{
				GSC.path target = end._paths.Find(x => x._id == origin._id);
				value._paths.Add(new GSC.path
				{
					_id = origin._id,
					_path = (lerpValue < 0.5f) ? origin._path : target._path,
				});
			}

			return value;
		}

		/// <summary>
		/// packs the message into the next best package
		/// </summary>
		/// <param name="maxPackageSize">the maximum size of packages</param>
		/// <param name="packages">the list of already existing packages</param>
		/// <param name="additionalMessage">the message that should be inserted</param>
		void HandlePackageSize(int maxPackageSize, List<byte[]> packages, byte[] additionalMessage)
		{
			int index = -1;
			int remainder = int.MaxValue;

			for(int i = 0; i < packages.Count; i++)
			{
				int currentRefmainter = maxPackageSize - (packages[i].Length + additionalMessage.Length);
				if(currentRefmainter >= 0 && currentRefmainter < remainder)
				{
					remainder = currentRefmainter;
					index = i;
				}
			}

			if(index < 0)
			{
				packages.Add(additionalMessage);
			}
			else
			{
				byte[] tmp = new byte[packages[index].Length + additionalMessage.Length];
				Buffer.BlockCopy(packages[index], 0, tmp, 0, packages[index].Length);
				Buffer.BlockCopy(additionalMessage, 0, tmp, packages[index].Length, additionalMessage.Length);
				packages[index] = tmp;
			}
		}
	}
}
