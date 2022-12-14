using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace PPBA
{
	namespace ISC //Input State Component
	{
		public class isc
		{
			public int _id;
			public int _client;
			public ObjectType _type;
		}

		[System.Serializable]
		public class obj : isc
		{
			public Vector3 _pos;
			public float _angle; //in degrees
		}

		[System.Serializable]
		public class combinedObj : isc
		{
			public Vector3[] _corners;
		}

		[System.Serializable]
		public class ProduceUnit
		{
			public int _client;
			public byte _pawnType;
			public byte _pawnCount;
		}
	}

	[System.Serializable]
	public class InputState
	{
		private static int _currentID = 0;
		public static readonly byte[] s_emptyInputState = { 0, 0, 0, 0, 0, 0, 0, 0, 0 };//remeber to update this if you change the encrypting

		public List<ISC.obj> _objs = new List<ISC.obj>();
		public List<ISC.combinedObj> _combinedObjs = new List<ISC.combinedObj>();
		public List<ISC.ProduceUnit> _produceUnits = new List<ISC.ProduceUnit>();

		/// <summary>
		/// adds a new object to the InputState
		/// </summary>
		/// <param name="type">type of the placed object</param>
		/// <param name="pos">position of the placed object</param>
		/// <param name="angle">rotation of the placed object around the y axis</param>
		/// <returns>input identifyer</returns>
		public int AddObj(ObjectType type, Vector3 pos, float angle)
		{
			ISC.obj value = new ISC.obj();
			value._id = _currentID++;
			value._type = type;
			value._pos = pos;
			value._angle = angle;
			_objs.Add(value);
			return value._id;
		}

		/// <summary>
		/// adds a wall or streat or trench or comparable to the InputState
		/// </summary>
		/// <param name="type">type of the placed object</param>
		/// <param name="corners">list of corner positions</param>
		/// <returns>input identifyer</returns>
		public int AddCombinedObj(ObjectType type, List<Vector3> corners)
		{
			ISC.combinedObj value = new ISC.combinedObj();
			value._id = _currentID++;
			value._type = type;
			value._corners = corners.ToArray();
			_combinedObjs.Add(value);
			return value._id;
		}

		/// <summary>
		/// Encrypts the data to sendable byte array
		/// </summary>
		/// <returns>the message</returns>
		public byte[] Encrypt()
		{
			List<byte> value = new List<byte>();

			value.AddRange(BitConverter.GetBytes(_objs.Count));
			foreach(var it in _objs)
			{
				value.AddRange(BitConverter.GetBytes(it._id));
				value.Add((byte)it._type);
				value.AddRange(BitConverter.GetBytes(it._pos.x));
				value.AddRange(BitConverter.GetBytes(it._pos.y));
				value.AddRange(BitConverter.GetBytes(it._pos.z));
				value.AddRange(BitConverter.GetBytes(it._angle));
			}

			value.AddRange(BitConverter.GetBytes(_combinedObjs.Count));
			foreach(var it in _combinedObjs)
			{
				value.AddRange(BitConverter.GetBytes(it._id));
				value.Add((byte)it._type);
				value.AddRange(BitConverter.GetBytes(it._corners.Length));
				for(int i = 0; i < it._corners.Length; i++)
				{
					value.AddRange(BitConverter.GetBytes(it._corners[i].x));
					value.AddRange(BitConverter.GetBytes(it._corners[i].y));
					value.AddRange(BitConverter.GetBytes(it._corners[i].z));
				}
			}

#if DB_IS
			Debug.Log(_produceUnits.Count + ", " + (byte)_produceUnits.Count);
#endif

			value.Add((byte)_produceUnits.Count);
			foreach(var it in _produceUnits)
			{
				value.Add(it._pawnType);
				value.Add(it._pawnCount);
			}

			//remember to update s_emptyInputState if you change something

#if DB_IS
			Debug.Log(value.Count);
#endif

			return value.ToArray();
		}

		/// <summary>
		/// Decypts an byte-array to usable data
		/// </summary>
		/// <param name="msg">the byte-array</param>
		/// <param name="offset">the start offset</param>
		/// <returns>the offset after the point it stoped reading</returns>
		public int Decrypt(int client, byte[] msg, int offset)
		{
#if DB_IS
			Debug.Log(offset + ", " + msg.Length);
#endif
			int count = BitConverter.ToInt32(msg, offset);
			offset += sizeof(int);

#if DB_IS
			Debug.Log(count);
#endif

			for(int i = 0; i < count; i++)
			{
				ISC.obj tmp = new ISC.obj();

				tmp._id = BitConverter.ToInt32(msg, offset);
				offset += sizeof(int);

				tmp._client = client;

				tmp._type = (ObjectType)msg[offset];
				offset++;

				tmp._pos.x = BitConverter.ToSingle(msg, offset);
				offset += sizeof(float);

				tmp._pos.y = BitConverter.ToSingle(msg, offset);
				offset += sizeof(float);

				tmp._pos.z = BitConverter.ToSingle(msg, offset);
				offset += sizeof(float);

				tmp._angle = BitConverter.ToSingle(msg, offset);
				offset += sizeof(float);

				_objs.Add(tmp);
			}

			count = BitConverter.ToInt32(msg, offset);
			offset += sizeof(int);

#if DB_IS
			Debug.Log(count);
#endif

			for(int i = 0; i < count; i++)
			{
				ISC.combinedObj tmp = new ISC.combinedObj();

				tmp._id = BitConverter.ToInt32(msg, offset);
				offset += sizeof(int);

				tmp._client = client;

				tmp._type = (ObjectType)msg[offset];
				offset++;

				tmp._corners = new Vector3[BitConverter.ToInt32(msg, offset)];
				offset += sizeof(int);

				for(int j = 0; j < tmp._corners.Length; j++)
				{
					tmp._corners[j].x = BitConverter.ToInt32(msg, offset);
					offset += sizeof(int);

					tmp._corners[j].y = BitConverter.ToInt32(msg, offset);
					offset += sizeof(int);

					tmp._corners[j].z = BitConverter.ToInt32(msg, offset);
					offset += sizeof(int);
				}

				_combinedObjs.Add(tmp);
			}

			count = msg[offset];
			offset++;

#if DB_IS
			Debug.Log(count);
#endif

			for(int i = 0; i < count; i++)
			{
				_produceUnits.Add(new ISC.ProduceUnit
				{
					_client = client,
					_pawnType = msg[offset],
					_pawnCount = msg[offset+1],
				});
				offset += 2;
			}

#if DB_IS
			Debug.Log("out offset: " + offset);
#endif

			return offset;
		}
	}
}
