using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.ComponentModel;
using System.Text;
using UnityEngine.Profiling;

namespace PPBA
{
	[System.Flags]
	public enum Arguments : byte
	{
		NON = 0,
		ENABLED = 1,//2^0
		TRIGGERBEHAVIOUR = 2,//2^1
	};

	struct HeatMapMessageElement
	{
		public BitField2D _mask;
		public List<float> _values;
	}

	namespace GSC //Game State Component
	{
		enum DataType : byte { NON, TYPE, ARGUMENT, TRANSFORM, AMMO, RESOURCE, HEALTH, WORK, BEHAVIOUR, ANIMATIONS, PATH, MAP, INPUTS, RANGE, PIXELWISE, BITMAPWISE };

		public class gsc
		{
			public int _id = 0;
		}

		[System.Serializable]
		public class type : gsc
		{
			public byte _type = 0;
			public byte _team = 0;

			public override string ToString()
			{
				return "ID: " + _id.ToString("0000") + "| type: " + _type + ", team: " + _team;
			}
		}

		[System.Serializable]
		public class arg : gsc
		{
			public Arguments _arguments = Arguments.NON;

			public override string ToString()
			{
				return "ID: " + _id.ToString("0000") + "| arguments: " + Convert.ToString((byte)_arguments, 2).PadLeft(8, '0');
			}
		}

		[System.Serializable]
		public class transform : gsc
		{
			public Vector3 _position = Vector3.zero;
			public float _angle = 0; //in degrees

			public override string ToString()
			{
				return "ID: " + _id.ToString("0000") + "| position: " + _position.ToString() + ", angle: " + _angle + "°";
			}
		}

		[System.Serializable]
		public class ammo : gsc
		{
			public int _bullets = 0;
			//public int _grenades;

			public override string ToString()
			{
				return "ID: " + _id.ToString("0000") + "| bullets: " + _bullets;
			}
		}

		[System.Serializable]
		public class resource : gsc
		{
			public int _resources = 0;

			public override string ToString()
			{
				return "ID: " + _id.ToString("0000") + "| resources: " + _resources;
			}
		}

		[System.Serializable]
		public class health : gsc
		{
			public float _health = 0;
			public float _morale = 0;//used for _score by depots/blueprints

			public override string ToString()
			{
				return "ID: " + _id.ToString("0000") + "| health: " + _health + ", moral: " + _morale;
			}
		}

		[System.Serializable]
		public class work : gsc
		{
			public int _work = 0;

			public override string ToString()
			{
				return "ID: " + _id.ToString("0000") + "| work: " + _work;
			}
		}

		[System.Serializable]
		public class behavior : gsc
		{
			public Behaviors _behavior = Behaviors.IDLE;
			public int _target = 0;

			public override string ToString()
			{
				return "ID: " + _id.ToString("0000") + "| behaviour: " + _behavior.ToString() + ", target-id: " + _target;
			}
		}

		[System.Serializable]
		public class animation : gsc
		{
			public PawnAnimations _animation;

			public override string ToString()
			{
				return _id.ToString("0000") + "| " + _animation.ToString();
			}
		}

		[System.Serializable]
		public class path : gsc
		{
			public Vector3[] _path = new Vector3[0];

			public override string ToString()
			{
				StringBuilder value = new StringBuilder();

				value.Append("ID: " + _id.ToString("0000"));
				value.Append("| path count: " + _path.Length);

				if(0 == _path.Length)
					return value.ToString();

				value.Append(" [");
				for(int i = 0; i < _path.Length - 1; i++)
				{
					value.Append(_path[i].ToString() + ", ");
				}
				value.Append(_path[_path.Length - 1].ToString() + "]");

				return value.ToString();
			}
		}

		public class heatMapElement
		{
			public byte _x;//Heatmap cant be bigger than 256
			public byte _y;//Heatmap cant be bigger than 256
			public float _value;
		}

		[System.Serializable]
		public class heatMap : gsc
		{
			public List<heatMapElement> _values = new List<heatMapElement>();

			public override string ToString()
			{
				StringBuilder value = new StringBuilder();
				value.Append("ID: " + _id.ToString("0000") + "| ");

				foreach(var it in _values)
				{
					value.Append("(" + it._x + " | " + it._y + ") = " + it._value + ", ");
				}

				return value.ToString();
			}
		}

		[System.Serializable]
		public class input : gsc
		{
			public int _client;

			public override string ToString()
			{
				return "ID: " + _id.ToString("0000") + "| for client: " + _client;
			}
		}

		[System.Serializable]
		public class newIDRange : gsc
		{
			public int _range;
			public ObjectType _type;

			public override string ToString()
			{
				return "ObjectPool: " + _type + "| ids: " + _id.ToString("0000") + " - " + (_id + _range).ToString("0000");
			}
		}
	}

	[System.Serializable]
	public class GameState
	{
		public GameState(bool isReceiverGameState = false)
		{
			_isEncrypted = isReceiverGameState;
		}

		/// <summary>
		/// copy GameState
		/// </summary>
		/// <param name="original">GameState to make copy from</param>
		public GameState(GameState original)
		{
			_refTick = original._refTick;
			_isLerped = original._isLerped;
			_isDelta = original._isDelta;
			_isEncrypted = original._isEncrypted;
			_receivedMessages = original._receivedMessages;
			_messageHolder = original._messageHolder;

			_types.AddRange(original._types.ToArray());
			_args.AddRange(original._args.ToArray());
			_transforms.AddRange(original._transforms.ToArray());
			_ammos.AddRange(original._ammos.ToArray());
			_resources.AddRange(original._resources.ToArray());
			_healths.AddRange(original._healths.ToArray());
			_works.AddRange(original._works.ToArray());
			_behaviors.AddRange(original._behaviors.ToArray());
			_animations.AddRange(original._animations.ToArray());
			_paths.AddRange(original._paths.ToArray());
			_heatMaps.AddRange(original._heatMaps.ToArray());
			_denyedInputIDs.AddRange(original._denyedInputIDs.ToArray());
			_newIDRanges.AddRange(original._newIDRanges.ToArray());
		}

		public int _refTick { get; set; } = 0;
		public bool _isLerped { get; private set; } = false;
		public bool _isDelta { get; private set; } = false;
		public bool _isEncrypted { get; private set; } = false;
		//public byte _messageCount;
		public BitField2D _receivedMessages = new BitField2D(0, 0);
		private List<byte[]> _messageHolder = null;
		//int _hash = 0;

		private List<GSC.type> _types = new List<GSC.type>();
		private List<GSC.arg> _args = new List<GSC.arg>();
		private List<GSC.transform> _transforms = new List<GSC.transform>();
		private List<GSC.ammo> _ammos = new List<GSC.ammo>();
		private List<GSC.resource> _resources = new List<GSC.resource>();
		private List<GSC.health> _healths = new List<GSC.health>();
		private List<GSC.work> _works = new List<GSC.work>();
		private List<GSC.behavior> _behaviors = new List<GSC.behavior>();
		private List<GSC.animation> _animations = new List<GSC.animation>();
		private List<GSC.path> _paths = new List<GSC.path>();
		private List<GSC.heatMap> _heatMaps = new List<GSC.heatMap>();
		///<summary>
		/// DO NOT USE
		/// </summary>
		public List<GSC.input> _denyedInputIDs = new List<GSC.input>();
		///<summary>
		/// DO NOT USE
		/// </summary>
		public List<GSC.newIDRange> _newIDRanges = new List<GSC.newIDRange>();

		public List<byte[]> Encrypt(int maxPackageSize)
		{
			if(_messageHolder != null)
				return _messageHolder;

			Profiler.BeginSample("[GameState] Encrypt");
			_messageHolder = new List<byte[]>();
			List<byte> msg = new List<byte>();

			//HeatMaps
			{
				Profiler.BeginSample("[GameState] heatMaps");
				if(_heatMaps.Count > 0)
				{
					/// byte Type, int ID, byte type, int pixelCount, {byte x, byte y, float value}[]
					/// byte Type, int ID, byte type, byte x, byte y, byte width, byte hight, byte[] mask, float[] values
					foreach(var it in _heatMaps)
					{


						byte xMin = byte.MaxValue;
						byte xSize = byte.MinValue;
						byte yMin = byte.MaxValue;
						byte ySize = byte.MinValue;

						foreach(var jt in it._values)
						{
							xMin = xMin < jt._x ? xMin : jt._x;
							xSize = jt._x < xSize ? jt._x : xSize;
							yMin = yMin < jt._y ? yMin : jt._y;
							ySize = jt._y < ySize ? jt._y : ySize;
						}
						xSize -= xMin;
						ySize -= yMin;

						if(it._values.Count * 2 < Mathf.CeilToInt((float)(xSize * ySize) / 8))
						{
							msg.Clear();
							msg.Add((byte)GSC.DataType.MAP);
							msg.AddRange(BitConverter.GetBytes(it._id));//overloads the count bytes in compareson to all other types
							msg.Add((byte)GSC.DataType.PIXELWISE);

							int maxElementCount = (maxPackageSize - 1 - sizeof(int)) / (2 + sizeof(float));

							int packageCount = Mathf.CeilToInt((float)it._values.Count / maxElementCount);

							if(packageCount == 0)
								continue;

							for(int i = 0; i < packageCount - 1; i++)
							{
								msg.AddRange(BitConverter.GetBytes(maxElementCount));

								for(int j = i * maxElementCount; j < i * maxElementCount + maxElementCount; j++)
								{
									msg.Add(it._values[j]._x);
									msg.Add(it._values[j]._y);
									msg.AddRange(BitConverter.GetBytes(it._values[j]._value));
								}

								HandlePackageSize(maxPackageSize, _messageHolder, msg.ToArray());

								msg.Clear();
								msg.Add((byte)GSC.DataType.MAP);
								msg.AddRange(BitConverter.GetBytes(it._id));//overloads the count bytes in compareson to all other types
								msg.Add((byte)GSC.DataType.PIXELWISE);
							}

							msg.AddRange(BitConverter.GetBytes(it._values.Count % maxElementCount));//should be the same value as the for loop count

							for(int i = (packageCount - 1) * maxElementCount; i < it._values.Count; i++)
							{
								msg.Add(it._values[i]._x);
								msg.Add(it._values[i]._y);
								msg.AddRange(BitConverter.GetBytes(it._values[i]._value));
							}

							HandlePackageSize(maxPackageSize, _messageHolder, msg.ToArray());
#if Obsolete

						msg.AddRange(BitConverter.GetBytes(it._values.Count));
						for(int i = 0; i < it._values.Count; i++)
						{
							msg.Add(it._values[i]._x);
							msg.Add(it._values[i]._y);
							msg.AddRange(BitConverter.GetBytes(it._values[i]._value));

							if(i %)
							{
								HandlePackageSize(maxPackageSize, _messageHolder, msg.ToArray());

								msg.Clear();
								msg.Add((byte)GSC.DataType.MAP);
								msg.AddRange(BitConverter.GetBytes(it._id));//overloads the count bytes in compareson to all other types
								msg.Add((byte)GSC.DataType.PIXELWISE);

								msg.AddRange(BitConverter.GetBytes(it._values.Count));
							}
						}
#endif
						}
						else
						{
							int xFieldCount = Mathf.CeilToInt((float)xSize / 64);
							int yFieldCount = Mathf.CeilToInt((float)ySize / 64);

							int bFWidth = Mathf.CeilToInt((float)xSize / xFieldCount);
							int bFHeight = Mathf.CeilToInt((float)ySize / yFieldCount);

							HeatMapMessageElement[,] messageHandler = new HeatMapMessageElement[xFieldCount, yFieldCount];
							for(int x = 0; x < xFieldCount; x++)
							{
								for(int y = 0; y < yFieldCount; y++)
								{
									messageHandler[x, y]._mask = new BitField2D(bFWidth, bFHeight);
									messageHandler[x, y]._values = new List<float>();
								}
							}
							foreach(var jt in it._values)
							{
								messageHandler[(jt._x - xMin) / bFWidth, (jt._y - yMin) / bFHeight]._mask[(jt._x - xMin) % bFWidth, (jt._y - yMin) % bFHeight] = true;
								messageHandler[(jt._x - xMin) / bFWidth, (jt._y - yMin) / bFHeight]._values.Add(jt._value);
							}

							for(int x = 0; x < xFieldCount; x++)
							{
								for(int y = 0; y < yFieldCount; y++)
								{
									if(messageHandler[x, y]._values.Count == 0)
										continue;

									msg.Clear();
									msg.Add((byte)GSC.DataType.MAP);
									msg.AddRange(BitConverter.GetBytes(it._id));//overloads the count bytes in compareson to all other types
									msg.Add((byte)GSC.DataType.BITMAPWISE);

									msg.Add((byte)(xMin + x * bFWidth));
									msg.Add((byte)(yMin + y * bFHeight));
									msg.Add((byte)bFWidth);
									msg.Add((byte)bFHeight);

									msg.AddRange(messageHandler[x, y]._mask.ToArray());
									for(int i = 0; i < messageHandler[x, y]._values.Count; i++)
									{
										msg.AddRange(BitConverter.GetBytes(messageHandler[x, y]._values[i]));
									}

									HandlePackageSize(maxPackageSize, _messageHolder, msg.ToArray());
								}
							}

#if Obsolete
							msg.Clear();
							msg.Add((byte)GSC.DataType.MAP);
							msg.AddRange(BitConverter.GetBytes(it._id));//overloads the count bytes in compareson to all other types
							msg.Add((byte)GSC.DataType.BITMAPWISE);

							msg.Add(xMin);
							msg.Add(yMin);
							msg.Add(xSize);
							msg.Add(ySize);

							BitField2D mask = new BitField2D(xSize, ySize);
							foreach(var jt in it._values)
							{
								mask[jt._x, jt._y] = true;
							}

							msg.AddRange(mask.ToArray());

							for(int i = 0; i < it._values.Count; i++)
							{
								msg.AddRange(BitConverter.GetBytes(it._values[i]._value));
							}
#endif
						}

						
					}
				}
				Profiler.EndSample();
			}
			//Paths
			{
				Profiler.BeginSample("[GameState] paths");
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

					HandlePackageSize(maxPackageSize, _messageHolder, msg.ToArray());
				}
				Profiler.EndSample();
			}
			//Args
			{
				Profiler.BeginSample("[GameState] args");
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

					HandlePackageSize(maxPackageSize, _messageHolder, msg.ToArray());
				}
				Profiler.EndSample();
			}
			//Transforms
			{
				Profiler.BeginSample("[GameState] transforms");
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

					HandlePackageSize(maxPackageSize, _messageHolder, msg.ToArray());
				}
				Profiler.EndSample();
			}
			//Resources
			{
				Profiler.BeginSample("[GameState] resources");
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

					HandlePackageSize(maxPackageSize, _messageHolder, msg.ToArray());
				}
				Profiler.EndSample();
			}
			//Animation
			{
				Profiler.BeginSample("[GameState] animations");
				if(_animations.Count > 0)
				{
					msg.Clear();
					msg.Add((byte)GSC.DataType.ANIMATIONS);
					msg.AddRange(BitConverter.GetBytes(_animations.Count));
					foreach(var it in _animations)
					{
						msg.AddRange(BitConverter.GetBytes(it._id));
						msg.Add((byte)it._animation);
					}

					HandlePackageSize(maxPackageSize, _messageHolder, msg.ToArray());
				}
				Profiler.EndSample();
			}

			//Behaviours
			{
				Profiler.BeginSample("[GameState] behaviors");
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

					HandlePackageSize(maxPackageSize, _messageHolder, msg.ToArray());
				}
				Profiler.EndSample();
			}
			//Health
			{
				Profiler.BeginSample("[GameState] healths");
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

					HandlePackageSize(maxPackageSize, _messageHolder, msg.ToArray());
				}
				Profiler.EndSample();
			}
			//Ammos
			{
				Profiler.BeginSample("[GameState] ammo");
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

					HandlePackageSize(maxPackageSize, _messageHolder, msg.ToArray());
				}
				Profiler.EndSample();
			}
			//Works
			{
				Profiler.BeginSample("[GameState] works");
				if(_works.Count > 0)
				{
					msg.Clear();
					msg.Add((byte)GSC.DataType.WORK);
					msg.AddRange(BitConverter.GetBytes(_works.Count));
					foreach(var it in _works)
					{
						msg.AddRange(BitConverter.GetBytes(it._id));
						msg.AddRange(BitConverter.GetBytes(it._work));
					}

					HandlePackageSize(maxPackageSize, _messageHolder, msg.ToArray());
				}
				Profiler.EndSample();
			}

			//Types
			{
				Profiler.BeginSample("[GameState] types");
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

					HandlePackageSize(maxPackageSize, _messageHolder, msg.ToArray());
				}
				Profiler.EndSample();
			}
			//Range
			{
				Profiler.BeginSample("[GameState] IDRanges");
				if(_newIDRanges.Count > 0)
				{
					msg.Clear();
					msg.Add((byte)GSC.DataType.RANGE);
					msg.AddRange(BitConverter.GetBytes(_newIDRanges.Count));
					foreach(var it in _newIDRanges)
					{
						msg.AddRange(BitConverter.GetBytes(it._id));
						msg.AddRange(BitConverter.GetBytes(it._range));
						msg.Add((byte)it._type);
					}

					HandlePackageSize(maxPackageSize, _messageHolder, msg.ToArray());
				}
				Profiler.EndSample();
			}
			//Input
			{
				Profiler.BeginSample("[GameState] denyedInput");
				if(_denyedInputIDs.Count > 0)
				{
					msg.Clear();
					msg.Add((byte)GSC.DataType.INPUTS);
					msg.AddRange(BitConverter.GetBytes(_denyedInputIDs.Count));
					foreach(var it in _denyedInputIDs)
					{
						msg.AddRange(BitConverter.GetBytes(it._id));
					}

					HandlePackageSize(maxPackageSize, _messageHolder, msg.ToArray());
				}
				Profiler.EndSample();
			}

			if(_messageHolder.Count == 0)
			{
				_messageHolder.Add(new byte[0]);
			}

			_receivedMessages = new BitField2D(_messageHolder.Count, 1);
			_isEncrypted = true;

			Profiler.EndSample();
			return _messageHolder;
		}

		public void Decrypt(byte[] msg, int offset, int packageNumber, int packageCount)
		{
			//Debug.Log("[GameState] package nr. " + packageNumber + " of " + packageCount);
			if(_receivedMessages.GetSize() == Vector2Int.zero)
			{
				//Debug.Log("[GameState] creating an bitfield ");
				_isEncrypted = true;
				_receivedMessages = new BitField2D(packageCount, 1);
			}
			if(_receivedMessages[packageNumber, 0])
				return;

			Profiler.BeginSample("[GameState] Decrypt");
			_receivedMessages[packageNumber, 0] = true;


			//_hash = BitConverter.ToInt32(msg, offset);
			//offset += sizeof(int);

			int count;
			while(offset < msg.Length)
			{
				//Debug.Log((GSC.DataType)msg[offset]);
				//Debug.Log(msg.Length + " | " + offset);
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

							value._health = BitConverter.ToSingle(msg, offset);
							offset += sizeof(float);

							value._morale = BitConverter.ToSingle(msg, offset);
							offset += sizeof(float);

							_healths.Add(value);
						}
						break;
					}
					case GSC.DataType.WORK:
					{
						_works = new List<GSC.work>(count);
						for(int i = 0; i < count; i++)
						{
							GSC.work value = new GSC.work();

							value._id = BitConverter.ToInt32(msg, offset);
							offset += sizeof(int);

							value._work = BitConverter.ToInt32(msg, offset);
							offset += sizeof(int);

							_works.Add(value);
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
					case GSC.DataType.ANIMATIONS:
					{
						_animations = new List<GSC.animation>(count);
						for(int i = 0; i < count; i++)
						{
							GSC.animation value = new GSC.animation();

							value._id = BitConverter.ToInt32(msg, offset);
							offset += sizeof(int);

							value._animation = (PawnAnimations)msg[offset];
							offset++;

							_animations.Add(value);
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
					case GSC.DataType.MAP:
					{
						GSC.heatMap value = _heatMaps.Find(x => x._id == count);
						if(null == value)
						{
							value = new GSC.heatMap();
							value._id = count;
						}

						GSC.DataType type = (GSC.DataType)msg[offset];
						offset++;

						switch(type)
						{
							/// byte Type, int ID, byte type| int pixelCount, {byte x, byte y, float value}[]
							case GSC.DataType.PIXELWISE:
								int size = BitConverter.ToInt32(msg, offset);
								offset += sizeof(int);

								for(int i = 0; i < size; i++)
								{
									GSC.heatMapElement element = new GSC.heatMapElement();

									element._x = msg[offset];
									offset++;

									element._y = msg[offset];
									offset++;

									element._value = BitConverter.ToSingle(msg, offset);
									offset += sizeof(float);

									value._values.Add(element);
								}
								break;
							/// byte Type, int ID, byte type| byte x, byte y, byte width, byte hight, byte[] mask, float[] values
							case GSC.DataType.BITMAPWISE:
								byte x = msg[offset];
								offset++;

								byte y = msg[offset];
								offset++;

								byte w = msg[offset];
								offset++;

								byte h = msg[offset];
								offset++;

								BitField2D mask = new BitField2D(w, h);

								byte[] field = mask.ToArray();
								Buffer.BlockCopy(msg, offset, field, 0, field.Length);
								offset += field.Length;
								mask.FromArray(field);

								Vector2Int[] pos = mask.GetActiveBits();
								float[] values = new float[pos.Length];

								Buffer.BlockCopy(msg, offset, values, 0, values.Length * sizeof(float));

								for(int i = 0; i < pos.Length; i++)
								{
									GSC.heatMapElement element = new GSC.heatMapElement();
									element._x = (byte)(pos[i].x + x);
									element._y = (byte)(pos[i].y + y);
									element._value = values[i];

									value._values.Add(element);
								}
								break;
							default:
								Debug.LogError("unable to read map " + value._id + " as type " + type);
								break;
						}

						_heatMaps.Add(value);
						break;
					}
					case GSC.DataType.INPUTS:
					{
						_denyedInputIDs = new List<GSC.input>(count);
						for(int i = 0; i < count; i++)
						{
							_denyedInputIDs.Add(new GSC.input { _id = BitConverter.ToInt32(msg, offset) });
							offset += sizeof(int);
						}
						break;
					}
					case GSC.DataType.RANGE:
					{
						_newIDRanges = new List<GSC.newIDRange>(count);
						for(int i = 0; i < count; i++)
						{
							GSC.newIDRange value = new GSC.newIDRange();

							value._id = BitConverter.ToInt32(msg, offset);
							offset += sizeof(int);

							value._range = BitConverter.ToInt32(msg, offset);
							offset += sizeof(int);

							value._type = (ObjectType)msg[offset];
							offset++;

							_newIDRanges.Add(value);
						}
						break;
					}
					default:
						throw new InvalidEnumArgumentException();
				}
			}

			if(_receivedMessages.AreAllBytesActive())
			{
				_isEncrypted = false;
			}

			Profiler.EndSample();
			//Debug.Log("----- EOM -----");
		}

		public bool CreateDelta(RingBuffer<GameState> references, int refTick, int myTick)
		{
			//_hash = GetHash();

			GameState reference = references[refTick];
			if(reference == null)
			{
				Debug.Log("reference not found. Tick: " + myTick + " | ref: " + refTick);
				return false;
			}

			Profiler.BeginSample("[GameState] Create Delta");

			_refTick = refTick;
			int removerIndex;

			removerIndex = 0;
			for(int i = 0; i < _types.Count; i++)
			{
				GSC.type element = reference._types.Find(x => x._id == _types[i]._id);

				if(null == element)
					goto Change;

				if(_types[i]._type != element._type)
					goto Change;
				if(_types[i]._team != element._team)
					goto Change;

				continue;
Change:
				_types[removerIndex] = _types[i];
				removerIndex++;
			}
			_types.RemoveRange(removerIndex, _types.Count - removerIndex);
			removerIndex = 0;
			for(int i = 0; i < _args.Count; i++)
			{
				GSC.arg element = reference._args.Find(x => x._id == _args[i]._id);

				if(null == element)
					goto Change;

				if(_args[i]._arguments != element._arguments)
					goto Change;

				continue;
Change:
				_args[removerIndex] = _args[i];
				removerIndex++;
			}
			_args.RemoveRange(removerIndex, _args.Count - removerIndex);
			removerIndex = 0;
			for(int i = 0; i < _transforms.Count; i++)
			{
				GSC.transform element = reference._transforms.Find(x => x._id == _transforms[i]._id);

				if(null == element)
					goto Change;

				if(_transforms[i]._position != element._position)
					goto Change;
				if(_transforms[i]._angle != element._angle)
					goto Change;

				continue;
Change:
				_transforms[removerIndex] = _transforms[i];
				removerIndex++;
			}
			_transforms.RemoveRange(removerIndex, _transforms.Count - removerIndex);
			removerIndex = 0;
			for(int i = 0; i < _ammos.Count; i++)
			{
				GSC.ammo element = reference._ammos.Find(x => x._id == _ammos[i]._id);

				if(null == element)
					goto Change;

				if(_ammos[i]._bullets != element._bullets)
					goto Change;

				continue;
Change:
				_ammos[removerIndex] = _ammos[i];
				removerIndex++;
			}
			_ammos.RemoveRange(removerIndex, _ammos.Count - removerIndex);
			removerIndex = 0;
			for(int i = 0; i < _resources.Count; i++)
			{
				GSC.resource element = reference._resources.Find(x => x._id == _resources[i]._id);

				if(null == element)
					goto Change;

				if(_resources[i]._resources != element._resources)
					goto Change;

				continue;
Change:
				_resources[removerIndex] = _resources[i];
				removerIndex++;
			}
			_resources.RemoveRange(removerIndex, _resources.Count - removerIndex);
			removerIndex = 0;
			for(int i = 0; i < _healths.Count; i++)
			{
				GSC.health element = reference._healths.Find(x => x._id == _healths[i]._id);

				if(null == element)
					goto Change;

				if(_healths[i]._health != element._health)
					goto Change;
				if(_healths[i]._morale != element._morale)
					goto Change;

				continue;
Change:
				_healths[removerIndex] = _healths[i];
				removerIndex++;
			}
			_healths.RemoveRange(removerIndex, _healths.Count - removerIndex);
			removerIndex = 0;
			for(int i = 0; i < _behaviors.Count; i++)
			{
				GSC.behavior element = reference._behaviors.Find(x => x._id == _behaviors[i]._id);

				if(null == element)
					goto Change;

				if(_behaviors[i]._behavior != element._behavior)
					goto Change;
				if(_behaviors[i]._target != element._target)
					goto Change;

				continue;
Change:
				_behaviors[removerIndex] = _behaviors[i];
				removerIndex++;
			}
			_behaviors.RemoveRange(removerIndex, _behaviors.Count - removerIndex);
			removerIndex = 0;
			for(int i = 0; i < _animations.Count; i++)
			{
				GSC.animation element = reference._animations.Find(x => x._id == _animations[i]._id);

				if(null == element)
					goto Change;

				if(_animations[i]._animation != element._animation)
					goto Change;

				continue;
Change:
				_animations[removerIndex] = _animations[i];
				removerIndex++;
			}
			_animations.RemoveRange(removerIndex, _animations.Count - removerIndex);
			removerIndex = 0;
			for(int i = 0; i < _paths.Count; i++)
			{
				GSC.path element = reference._paths.Find(x => x._id == _paths[i]._id);

				if(null == element)
					goto Change;

				if(_paths[i]._path.Length != element._path.Length)
					goto Change;

				for(int j = 0; j < _paths[i]._path.Length; j++)
				{
					if(_paths[i]._path[j] != element._path[j])
						continue;
				}
Change:
				_paths[removerIndex] = _paths[i];
				removerIndex++;
			}
			_paths.RemoveRange(removerIndex, _paths.Count - removerIndex);
			Profiler.BeginSample("[GameState] backing");
			removerIndex = 0;
			for(int i = 0; i < _heatMaps.Count; i++)
			{

				GSC.heatMap lastDelta = null;
				for(int j = myTick - 1; null == lastDelta && j > refTick; j--)
				{
					if(j < references.GetLowEnd() || j > references.GetHighEnd())
						continue;
					if(references[j] == default)
						continue;
					if(references[j]._heatMaps == null)
						continue;

					lastDelta = references[j]._heatMaps.Find(x => x._id == _heatMaps[i]._id);
				}

				if(lastDelta != null)
				{
					Add(lastDelta, false);
					//foreach(var it in lastDelta._values)
					//{
					//	if(!_heatMaps[i]._values.Exists(x => x._x == it._x && x._y == it._y)){
					//		_heatMaps[i]._values.Add(it);
					//	}
					//}
				}

				GSC.heatMap element = reference._heatMaps.Find(x => x._id == _heatMaps[i]._id);

				if(null == element)
					goto Change;

				if(_heatMaps[i]._values.Count != element._values.Count)
					goto Change;

				int elemenetRemoverIndex = 0;
				for(int j = 0; j < element._values.Count; j++)
				{
					GSC.heatMapElement elementElement = element._values.Find(x => x._x == _heatMaps[i]._values[j]._x && x._y == _heatMaps[i]._values[j]._y);
					if(null == elementElement)
						goto ChangeElement;
					if(_heatMaps[i]._values[j]._value != elementElement._value)
						goto ChangeElement;

					continue;

ChangeElement:
					_heatMaps[i]._values[elemenetRemoverIndex] = _heatMaps[i]._values[j];
					elemenetRemoverIndex++;
				}
				_heatMaps[i]._values.RemoveRange(elemenetRemoverIndex, _heatMaps[i]._values.Count - elemenetRemoverIndex);

				if(_heatMaps[i]._values.Count > 0)
					goto Change;

				continue;
Change:
				_heatMaps[removerIndex] = _heatMaps[i];
				removerIndex++;
			}
			_heatMaps.RemoveRange(removerIndex, _heatMaps.Count - removerIndex);
			for(int i = refTick + 1; i < myTick; i++)
			{
				//Debug.Log("[GameState] searching for denyed inputs in tick: " + i);
				GameState nextState = references[i];
				if(nextState == default)
				{
					//Debug.Log("[GameState] tick was not calculated");
					continue;
				}

				//Debug.Log("[GameState] adding denyed inputs");
				_denyedInputIDs.AddRange(nextState._denyedInputIDs.FindAll(x => !_denyedInputIDs.Exists(y => y._id == x._id)));
			}
			_denyedInputIDs.RemoveAll(x => reference._denyedInputIDs.Exists(y => y._id == x._id));
			//Debug.Log("[GameState] finished denyed input backing");
			for(int i = refTick + 1; i < myTick; i++)
			{
				GameState nextState = references[i];
				if(nextState == default)
				{
					continue;
				}

				_newIDRanges.AddRange(nextState._newIDRanges.FindAll(x => !_newIDRanges.Exists(y => y._id == x._id)));
			}
			_newIDRanges.RemoveAll(x => reference._newIDRanges.Exists(y => y._id == x._id));
			Profiler.EndSample();

			_isDelta = true;

			Profiler.EndSample();

			return true;
		}

		public bool DismantleDelta(GameState reference)
		{
			if(reference == default)
				return false;

			Profiler.BeginSample("[GameState] Dismantle Delta");

			//_messageHolder = null;

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
			foreach(var it in reference._animations)
			{
				if(_animations.Exists(x => x._id == it._id))
					continue;

				_animations.Add(it);
			}
			foreach(var it in reference._paths)
			{
				if(_paths.Exists(x => x._id == it._id))
					continue;

				_paths.Add(it);
			}

			/*if(_hash != GetHash())
			{
				Debug.LogError("hashes are unequal: " + _hash + " <-> " + GetHash());
			}*/

			_isDelta = false;

			Profiler.EndSample();

			return true;
		}

		public static GameState Lerp(GameState start, GameState end, float lerpValue)
		{
			Profiler.BeginSample("[GameState] Lerp");
			GameState value = new GameState();

			foreach(var origin in start._types)
			{
				GSC.type target = end._types.Find(x => x._id == origin._id);
				if(target == default)
					continue;

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
				if(target == default)
					continue;

				value._args.Add(new GSC.arg
				{
					_id = origin._id,
					_arguments = (lerpValue < 0.5f) ? origin._arguments : target._arguments,
				});//Flags könnten verlohren gene, vorallem wenn meherere ticks auf einmal geschickt werden.
			}
			foreach(var origin in start._transforms)
			{
				GSC.transform target = end._transforms.Find(x => x._id == origin._id);
				if(target == default)
					continue;

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
				if(target == default)
					continue;

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
				if(target == default)
					continue;

				value._resources.Add(new GSC.resource
				{
					_id = origin._id,
					_resources = (int)Mathf.Lerp(origin._resources, target._resources, lerpValue),
				});
			}
			foreach(var origin in start._healths)
			{
				GSC.health target = end._healths.Find(x => x._id == origin._id);
				if(target == default)
					continue;

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
				if(target == default)
					continue;

				value._behaviors.Add(new GSC.behavior
				{
					_id = origin._id,
					_behavior = (lerpValue < 0.5f) ? origin._behavior : target._behavior,
					_target = (lerpValue < 0.5f) ? origin._target : target._target,
				});
			}
			foreach(var origin in start._animations)
			{
				GSC.animation target = end._animations.Find(x => x._id == origin._id);
				if(target == default)
					continue;

				value._animations.Add(new GSC.animation
				{
					_id = origin._id,
					_animation = (lerpValue < 0.5f) ? origin._animation : target._animation,
				});
			}
			foreach(var origin in start._paths)
			{
				GSC.path target = end._paths.Find(x => x._id == origin._id);
				if(target == default)
					continue;

				value._paths.Add(new GSC.path
				{
					_id = origin._id,
					_path = (lerpValue < 0.5f) ? origin._path : target._path,
				});
			}

			value._heatMaps.AddRange(((lerpValue < 0.5f) ? start : end)._heatMaps);

			value._denyedInputIDs = end._denyedInputIDs;
			value._newIDRanges = end._newIDRanges;

			value._refTick = start._refTick;
			value._isEncrypted = false;
			value._isDelta = false;
			value._isLerped = true;

			Profiler.EndSample();
			return value;
		}

#region Helper Funktion

		public GSC.type GetType(int id) => _types.Find(x => x._id == id);
		public GSC.arg GetArg(int id) => _args.Find(x => x._id == id);
		public GSC.transform GetTransform(int id) => _transforms.Find(x => x._id == id);
		public GSC.ammo GetAmmo(int id) => _ammos.Find(x => x._id == id);
		public GSC.resource GetResource(int id) => _resources.Find(x => x._id == id);
		public GSC.health GetHealth(int id) => _healths.Find(x => x._id == id);
		public GSC.work GetWork(int id) => _works.Find(x => x._id == id);
		public GSC.behavior GetBehavior(int id) => _behaviors.Find(x => x._id == id);
		public GSC.animation GetAnimation(int id) => _animations.Find(x => x._id == id);
		public GSC.path GetPath(int id) => _paths.Find(x => x._id == id);
		public GSC.heatMap GetHeatMap(int id) => _heatMaps.Find(x => x._id == id);
		public GSC.input GetInput(int id) => _denyedInputIDs.Find(x => x._id == id);
		public GSC.newIDRange GetNewIDRange(int id) => _newIDRanges.Find(x => x._id == id);

		public void Add(GSC.type element)
		{
			if(_types.Exists(x => x._id == element._id))
			{
				Debug.LogWarning("Type allready exists: " + element.ToString());
				return;
			}

			_types.Add(element);
		}

		public void Add(GSC.arg element)
		{
			GSC.arg value = _args.Find(x => x._id == element._id);
			if(null == value)
			{
				_args.Add(element);
			}
			else
			{
				Debug.LogWarning("Arg allready exists: " + element.ToString());
				value._arguments |= element._arguments;
			}
		}

		public void Add(GSC.transform element)
		{
			if(_transforms.Exists(x => x._id == element._id))
			{
				Debug.LogWarning("Transform allready exists: " + element.ToString());
				return;
			}

			_transforms.Add(element);
		}

		public void Add(GSC.ammo element)
		{
			if(_ammos.Exists(x => x._id == element._id))
			{
				Debug.LogWarning("Ammo allready exists: " + element.ToString());
				return;
			}

			_ammos.Add(element);
		}

		public void Add(GSC.resource element)
		{
			if(_resources.Exists(x => x._id == element._id))
			{
				Debug.LogWarning("Resource allready exists: " + element.ToString());
				return;
			}

			_resources.Add(element);
		}

		public void Add(GSC.health element)
		{
			if(_healths.Exists(x => x._id == element._id))
			{
				Debug.LogWarning("Health allready exists: " + element.ToString());
				return;
			}

			_healths.Add(element);
		}

		public void Add(GSC.work element)
		{
			if(_works.Exists(x => x._id == element._id))
			{
				Debug.LogWarning("Work allready exists: " + element.ToString());
				return;
			}

			_works.Add(element);
		}

		public void Add(GSC.behavior element)
		{
			if(_behaviors.Exists(x => x._id == element._id))
			{
				Debug.LogWarning("Behaviour allready exists: " + element.ToString());
				return;
			}

			_behaviors.Add(element);
		}

		public void Add(GSC.animation element)
		{
			if(_animations.Exists(x => x._id == element._id))
			{
				Debug.LogWarning("Animation already exists: " + element.ToString());
				return;
			}

			_animations.Add(element);
		}

		public void Add(GSC.path element)
		{
			if(_paths.Exists(x => x._id == element._id))
			{
				Debug.LogWarning("Path allready exists: " + element.ToString());
				return;
			}

			_paths.Add(element);
		}

		public void Add(GSC.heatMap element, bool isMoreSignificant = true)
		{
			if(null == element._values)
			{
				return;
			}

			GSC.heatMap target = _heatMaps.Find(x => x._id == element._id);

			if(null == target)
			{
				_heatMaps.Add(element);
			}
			else
			{
				Debug.LogWarning("HeatMap allready exists: " + element.ToString());

				foreach(var it in element._values)
				{
					GSC.heatMapElement tmp = target._values.Find(x => x._x == it._x && x._y == it._y);
					if(null == tmp)
					{
						target._values.Add(it);
					}
					else
					{
						tmp._value = isMoreSignificant ? it._value : tmp._value;
					}
				}
			}
		}

		public void Add(GSC.input element)
		{
			if(_denyedInputIDs.Exists(x => x._id == element._id))
			{
				Debug.LogWarning("Input allready exists: " + element.ToString());
				return;
			}

			_denyedInputIDs.Add(element);
		}

		public void Add(GSC.newIDRange element)
		{
			if(_newIDRanges.Exists(x => x._id == element._id))
			{
				Debug.LogWarning("IDs allready exists: " + element.ToString());
				return;
			}

			_newIDRanges.Add(element);
		}
#endregion

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

		int GetHash()
		{
			Profiler.BeginSample("[GameState] Hash");
			int hash = 0;

			for(int i = 0; i < _types.Count; i++)
			{
				hash += _types[i]._id;
				hash += (hash << 2);
				hash ^= (hash >> 6);
				hash += _types[i]._team;
				hash += (hash << 2);
				hash ^= (hash >> 6);
				hash += _types[i]._type;
				hash += (hash << 2);
				hash ^= (hash >> 6);
			}
			for(int i = 0; i < _args.Count; i++)
			{
				hash += _args[i]._id;
				hash += (hash << 2);
				hash ^= (hash >> 6);
				hash += (byte)_args[i]._arguments;
				hash += (hash << 2);
				hash ^= (hash >> 6);
			}
			for(int i = 0; i < _transforms.Count; i++)
			{
				hash += _transforms[i]._id;
				hash += (hash << 2);
				hash ^= (hash >> 6);
				hash += BitConverter.ToInt32(BitConverter.GetBytes(_transforms[i]._position.x), 0);
				hash += (hash << 2);
				hash ^= (hash >> 6);
				hash += BitConverter.ToInt32(BitConverter.GetBytes(_transforms[i]._position.y), 0);
				hash += (hash << 2);
				hash ^= (hash >> 6);
				hash += BitConverter.ToInt32(BitConverter.GetBytes(_transforms[i]._position.z), 0);
				hash += (hash << 2);
				hash ^= (hash >> 6);
				hash += BitConverter.ToInt32(BitConverter.GetBytes(_transforms[i]._angle), 0);
				hash += (hash << 2);
				hash ^= (hash >> 6);
			}
			for(int i = 0; i < _ammos.Count; i++)
			{
				hash += _ammos[i]._id;
				hash += (hash << 2);
				hash ^= (hash >> 6);
				hash += _ammos[i]._bullets;
				hash += (hash << 2);
				hash ^= (hash >> 6);
			}
			for(int i = 0; i < _resources.Count; i++)
			{
				hash += _resources[i]._id;
				hash += (hash << 2);
				hash ^= (hash >> 6);
				hash += _resources[i]._resources;
				hash += (hash << 2);
				hash ^= (hash >> 6);
			}
			for(int i = 0; i < _healths.Count; i++)
			{
				hash += _healths[i]._id;
				hash += (hash << 2);
				hash ^= (hash >> 6);
				hash += BitConverter.ToInt32(BitConverter.GetBytes(_healths[i]._health), 0);
				hash += (hash << 2);
				hash ^= (hash >> 6);
				hash += BitConverter.ToInt32(BitConverter.GetBytes(_healths[i]._morale), 0);
				hash += (hash << 2);
				hash ^= (hash >> 6);
			}
			for(int i = 0; i < _works.Count; i++)
			{
				hash += _works[i]._id;
				hash += (hash << 2);
				hash ^= (hash >> 6);
				hash += _works[i]._work;
				hash += (hash << 2);
				hash ^= (hash >> 6);
			}
			for(int i = 0; i < _behaviors.Count; i++)
			{
				hash += _behaviors[i]._id;
				hash += (hash << 2);
				hash ^= (hash >> 6);
				hash += (byte)_behaviors[i]._behavior;
				hash += (hash << 2);
				hash ^= (hash >> 6);
				hash += _behaviors[i]._target;
				hash += (hash << 2);
				hash ^= (hash >> 6);
			}

			Debug.Log("Hash: " + hash);

			Profiler.EndSample();

			return hash;
		}

		public override string ToString()
		{
			StringBuilder value = new StringBuilder();

			value.AppendLine("reference tick is:- - - " + _refTick);
			value.AppendLine("is Delta: - - - - - - - " + _isDelta);
			value.AppendLine("is Lerped:- - - - - - - " + _isLerped);
			value.AppendLine("is Encrypted: - - - - - " + _isEncrypted);
			value.AppendLine("packages where created: " + (null != _messageHolder));
			if(null != _messageHolder)
			{
				value.AppendLine("total amount of messages: " + _receivedMessages.GetSize().x);
				for(int i = 0; i < _receivedMessages.GetSize().x; i++)
				{
					value.AppendLine("package " + i + " was " + (_receivedMessages[i, 0] ? "receaved" : "not receaved"));
				}
			}

			value.AppendLine();

			value.AppendLine("type count:- - - - - " + _types.Count);
			foreach(var it in _types)
			{
				value.AppendLine(it.ToString());
			}

			value.AppendLine("argument count:- - - " + _args.Count);
			foreach(var it in _args)
			{
				value.AppendLine(it.ToString());
			}

			value.AppendLine("transform count: - - " + _transforms.Count);
			foreach(var it in _transforms)
			{
				value.AppendLine(it.ToString());
			}

			value.AppendLine("ammo count:- - - - - " + _ammos.Count);
			foreach(var it in _ammos)
			{
				value.AppendLine(it.ToString());
			}

			value.AppendLine("resource count:- - - " + _resources.Count);
			foreach(var it in _resources)
			{
				value.AppendLine(it.ToString());
			}

			value.AppendLine("health count:- - - - " + _healths.Count);
			foreach(var it in _healths)
			{
				value.AppendLine(it.ToString());
			}

			value.AppendLine("work count:- - - - - " + _works.Count);
			foreach(var it in _works)
			{
				value.AppendLine(it.ToString());
			}

			value.AppendLine("behavior count:- - - " + _behaviors.Count);
			foreach(var it in _behaviors)
			{
				value.AppendLine(it.ToString());
			}

			value.AppendLine("path count:- - - - - " + _paths.Count);
			foreach(var it in _paths)
			{
				value.AppendLine(it.ToString());
			}

			value.AppendLine("heatMap count: - - - " + _heatMaps.Count);
			foreach(var it in _heatMaps)
			{
				value.AppendLine(it.ToString());
			}

			value.AppendLine("denyedInputID count: " + _denyedInputIDs.Count);
			foreach(var it in _denyedInputIDs)
			{
				value.AppendLine(it.ToString());
			}

			value.AppendLine("newIDRange count:- - " + _newIDRanges.Count);
			foreach(var it in _newIDRanges)
			{
				value.AppendLine(it.ToString());
			}

			return value.ToString();
		}
	}
}
