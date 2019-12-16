using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.ComponentModel;
using System.Text;

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
		enum DataType : byte { NON, TYPE, ARGUMENT, TRANSFORM, AMMO, RESOURCE, HEALTH, WORK, BEHAVIOUR, PATH, MAP, INPUTS, RANGE };

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
				return "ID: " + _id.ToString("0000") + "| arguments: " + _arguments.ToString();
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

		[System.Serializable]
		public class heatMap : gsc
		{
			public BitField2D _mask = new BitField2D(0, 0);
			public List<float> _values = new List<float>();

			public override string ToString()
			{
				StringBuilder value = new StringBuilder();
				value.AppendLine("ID: " + _id.ToString("0000"));

				Vector2Int size = _mask.GetSize();
				int valueIndex = 0;
				for(int y = 0; y < size.y; y++)
				{
					for(int x = 0; x < size.x; x++)
					{
						if(_mask[x, y])
						{
							value.Append(" " + _values[valueIndex].ToString("0.000") + " ");
							valueIndex++;
						}
						else
						{
							value.Append(" XXXXX ");
						}
					}
					value.AppendLine();
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
		public GameState()
		{

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

		public List<GSC.type> _types = new List<GSC.type>();
		public List<GSC.arg> _args = new List<GSC.arg>();
		public List<GSC.transform> _transforms = new List<GSC.transform>();
		public List<GSC.ammo> _ammos = new List<GSC.ammo>();
		public List<GSC.resource> _resources = new List<GSC.resource>();
		public List<GSC.health> _healths = new List<GSC.health>();
		public List<GSC.work> _works = new List<GSC.work>();
		public List<GSC.behavior> _behaviors = new List<GSC.behavior>();
		public List<GSC.path> _paths = new List<GSC.path>();
		public List<GSC.heatMap> _heatMaps = new List<GSC.heatMap>();
		public List<GSC.input> _denyedInputIDs = new List<GSC.input>();
		public List<GSC.newIDRange> _newIDRanges = new List<GSC.newIDRange>();

		public List<byte[]> Encrypt(int maxPackageSize)
		{
			if(_messageHolder != null)
				return _messageHolder;

			_messageHolder = new List<byte[]>();
			List<byte> msg = new List<byte>();

			//HandlePackageSize(maxPackageSize, value, BitConverter.GetBytes(_hash));

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
			if(_heatMaps.Count > 0)
			{
				foreach(var it in _heatMaps)
				{
					msg.Clear();
					msg.Add((byte)GSC.DataType.MAP);
					msg.AddRange(BitConverter.GetBytes(it._id));//overloads the count bytes in compareson to all other types
					msg.AddRange(it._mask.ToArray());
					msg.AddRange(BitConverter.GetBytes(it._values.Count));
					for(int i = 0; i < it._values.Count; i++)
					{
						msg.AddRange(BitConverter.GetBytes(it._values[i]));
					}

					HandlePackageSize(maxPackageSize, _messageHolder, msg.ToArray());
				}
			}
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

			if(_messageHolder.Count == 0)
			{
				_messageHolder.Add(new byte[0]);
			}

			/*{
				_types.Clear();
				_args.Clear();
				_transforms.Clear();
				_ammos.Clear();
				_resources.Clear();
				_healths.Clear();
				_works.Clear();
				_behaviors.Clear();
				_paths.Clear();
				_heatMaps.Clear();
				_denyedInputIDs.Clear();
				_newIDRanges.Clear();
			}*/
			_receivedMessages = new BitField2D(_messageHolder.Count, 1);
			_isEncrypted = true;
			return _messageHolder;
		}

		public void Decrypt(byte[] msg, int offset, int packageNumber, int packageCount)
		{
			//Debug.Log("[GameState] package nr. " + packageNumber + " of " + packageCount);
			if(_receivedMessages.GetSize() == Vector2Int.zero)
			{
				//Debug.Log("[GameState] creating an bitfield ");
				_receivedMessages = new BitField2D(packageCount, 1);
			}
			if(_receivedMessages[packageNumber, 0])
				return;

			_receivedMessages[packageNumber, 0] = true;


			//_hash = BitConverter.ToInt32(msg, offset);
			//offset += sizeof(int);

			int count;
			while(offset < msg.Length)
			{
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
						GSC.heatMap value = new GSC.heatMap();
						value._id = count;//value overload

						Vector2Int space = HeatMapHandler.s_instance.GetHeatMapSize(value._id);
						value._mask = new BitField2D(space.x, space.y);

						byte[] mask = value._mask.ToArray();
						Buffer.BlockCopy(msg, offset, mask, 0, mask.Length);
						offset += mask.Length;

						value._mask.FromArray(mask);


						int size = value._mask.GetActiveBits().Length;
						value._values = new List<float>(size);
						for(int j = 0; j < size; j++)
						{
							value._values.Add(BitConverter.ToSingle(msg, offset));
							offset += sizeof(float);
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
						}
						break;
					}
					default:
						throw new InvalidEnumArgumentException();
				}
			}

			if(_receivedMessages.AreAllBytesActive())
				_isEncrypted = false;
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

			//Debug.Log(myTick + ": " + _isDelta + ", " + _isEncrypted + ", " + _isLerped);
			//Debug.Log(refTick + ": " + reference._isDelta + ", " + reference._isEncrypted + ", " + reference._isLerped);

			_refTick = refTick;

			foreach(var it in reference._types)
			{
				GSC.type element = _types.Find(x => x._id == it._id);
				if(element == null)
					continue;

				if(it._type != element._type)
					continue;
				if(it._team != element._team)
					continue;

				_types.Remove(element);
			}
			//Debug.Log(reference._args.Count + " | " + _args.Count);
			foreach(var it in reference._args)
			{
				GSC.arg element = _args.Find(x => x._id == it._id);
				if(element == null)
					continue;

				//Debug.Log(it._arguments + " | " + element._arguments);

				if(it._arguments != element._arguments)
					continue;

				//Debug.Log(element + " will be removed");

				_args.Remove(element);
			}
			foreach(var it in reference._transforms)
			{
				GSC.transform element = _transforms.Find(x => x._id == it._id);
				if(element == null)
					continue;

				if(it._position != element._position)
					continue;
				if(it._angle != element._angle)
					continue;

				_transforms.Remove(element);
			}
			foreach(var it in reference._ammos)
			{
				GSC.ammo element = _ammos.Find(x => x._id == it._id);
				if(element == null)
					continue;

				if(it._bullets != element._bullets)
					continue;
				/*if(it._grenades != element._grenades)
					continue;*/

				_ammos.Remove(element);
			}
			foreach(var it in reference._resources)
			{
				GSC.resource element = _resources.Find(x => x._id == it._id);
				if(element == null)
					continue;

				if(it._resources != element._resources)
					continue;

				_resources.Remove(element);
			}
			foreach(var it in reference._healths)
			{
				GSC.health element = _healths.Find(x => x._id == it._id);
				if(element == null)
					continue;

				if(it._health != element._health)
					continue;
				if(it._morale != element._morale)
					continue;

				_healths.Remove(element);
			}
			foreach(var it in reference._behaviors)
			{
				GSC.behavior element = _behaviors.Find(x => x._id == it._id);
				if(element == null)
					continue;

				if(it._behavior != element._behavior)
					continue;
				if(it._target != element._target)
					continue;

				_behaviors.Remove(element);
			}
			foreach(var it in reference._paths)
			{
				GSC.path element = _paths.Find(x => x._id == it._id);
				if(element == null)
					continue;

				if(it._path.Length != element._path.Length)
					continue;

				/*
				 * {
				 * 	int i = 0;
				 * 	for(; i < it._path.Count; i++)
				 * 	{
				 * 		if(it._path[i] != element._path[i])
				 * 			break;
				 * 	}
				 * 	if(i < it._path.Count)
				 * 		continue;
				 * }
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
			foreach(var it in _heatMaps)
			{
				//escape if reference tick dosn't have the heatmap
				GSC.heatMap refMap = reference._heatMaps.Find(x => x._id == it._id);
				if(null == refMap)
					continue;

				//baking Bitfield
				for(int i = refTick + 1; i < myTick; i++)
				{
					GameState nextState = references[i];
					if(nextState == default)
						continue;

					BitField2D tmp = references[i]._heatMaps.Find(x => x._id == it._id)?._mask;
					if(null == tmp)
						continue;

					it._mask += references[i]._heatMaps.Find(x => x._id == it._id)._mask;
				}

				//getting values
				Vector2Int[] refPos = refMap._mask.GetActiveBits();

				Vector2Int[] positions = it._mask.GetActiveBits();
				List<float> values = new List<float>(positions.Length);

				for(int i = 0, j = 0; i < positions.Length; i++)
				{
					//finde gleiches element (hör auf zu suchen, wenn es größer ist)
					for(; j < refPos.Length; j++)
					{
						if(refPos[j].y >= positions[i].y && refPos[j].x >= positions[i].x)
							break;
					}

					//wenn gleiches element existiert
					if(refPos[j] == positions[i])
					{
						float value = it._values[i];

						//ist der float wert in _value von refMap an dem index des gleichen elements gleich zu dem value in der map
						if(refMap._values[j] == value)
							it._mask[positions[i].x, positions[i].y] = false;
						else
							values.Add(value);
					}
				}
				it._values = values;
			}
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

			_isDelta = true;

			return true;
		}

		public bool DismantleDelta(GameState reference)
		{
			if(reference == default)
				return false;

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

			return true;
		}

		public static GameState Lerp(GameState start, GameState end, float lerpValue)
		{
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
			foreach(var it in start._heatMaps)
			{
				value._heatMaps.Add((lerpValue < 0.5f) ? it : end._heatMaps.Find(x => x._id == it._id));
			}
			value._denyedInputIDs = end._denyedInputIDs;
			value._newIDRanges = end._newIDRanges;

			value._refTick = start._refTick;
			value._isEncrypted = false;
			value._isDelta = false;
			value._isLerped = true;

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
		public GSC.path GetPath(int id) => _paths.Find(x => x._id == id);
		public GSC.heatMap GetHeatMap(int id) => _heatMaps.Find(x => x._id == id);
		public GSC.input GetInput(int id) => _denyedInputIDs.Find(x => x._id == id);
		public GSC.newIDRange GetNewIDRange(int id) => _newIDRanges.Find(x => x._id == id);

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
