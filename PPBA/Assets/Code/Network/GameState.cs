using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.ComponentModel;

namespace PPBA
{
	public enum Behavior : byte {IDLE };

	[System.Flags]
	public enum Arguments : byte {
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

		public class behavior : gsc
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
			public List<float> _values;
		}
	}
	public class GameState
	{
		public int _refTick = -1;
		public byte _messageCount;
		public BitField2D _receivedMessages;
		public bool _isLerped;
		public bool _isDelta;
		private List<byte[]> _messageHolder = null;

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
					msg.AddRange(BitConverter.GetBytes(it._path.Count));
					for(int i = 0; i < it._path.Count; i++)
					{
						msg.AddRange(BitConverter.GetBytes(it._path[i].x));
						msg.AddRange(BitConverter.GetBytes(it._path[i].y));
						msg.AddRange(BitConverter.GetBytes(it._path[i].z));
					}
				}

				HandlePackageSize(maxPackageSize, value, msg.ToArray());
			}
			if(_maps.Count > 0)
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
			}
			if(_denyedInputIDs.Count > 0)
			{
				msg.Clear();
				msg.Add((byte)GSC.DataType.INPUTS);
				msg.AddRange(BitConverter.GetBytes(_denyedInputIDs.Count));
				foreach(var it in _denyedInputIDs)
				{
					msg.AddRange(BitConverter.GetBytes(it));
				}

				HandlePackageSize(maxPackageSize, value, msg.ToArray());
			}

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

							value._behavior = (Behavior)msg[offset];
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

							value._path = new List<Vector3>(size);
							for(int j = 0; j < size; j++)
							{
								value._path.Add(new Vector3(
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
					case GSC.DataType.MAP://TODO: Renes HeatMap
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
					}
					case GSC.DataType.INPUTS:
					{
						_denyedInputIDs = new List<int>(count);
						for(int i = 0; i < count; i++)
						{
							_denyedInputIDs.Add(BitConverter.ToInt32(msg, offset));
							offset += sizeof(int);
						}
						break;
					}
					default:
						throw new InvalidEnumArgumentException();
				}
			}
		}

		public bool CreateDelta(RingBuffer<GameState> reference, int tick, int length)
		{
			throw new NotImplementedException();
		}

		public bool DismantleDelta(GameState reference, List<int> expactedInputs)
		{
			throw new NotImplementedException();
		}

		public bool Lerp(GameState start, GameState end, int t)
		{
			throw new NotImplementedException();
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
