using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace PPBA
{
	public class BitField2D
	{
		private byte[] _backingArray = null;
		private int _fieldWidth = -1;
		private int _fieldHight = -1;

		public BitField2D(int width, int hight)
		{
			_backingArray = new byte[Mathf.CeilToInt((width * hight) / 8.0f)];
			_fieldWidth = width;
			_fieldHight = hight;
		}

		public BitField2D(int width, int hight, byte[] values)
		{
			_backingArray = new byte[Mathf.CeilToInt((width * hight) / 8.0f)];
			_fieldWidth = width;
			_fieldHight = hight;
			Buffer.BlockCopy(values, 0, _backingArray, 0, Mathf.CeilToInt((width * hight) / 8.0f));
		}

		public BitField2D(BitField2D origin, bool doEmpty)
		{
			_fieldWidth = origin._fieldWidth;
			_fieldHight = origin._fieldHight;
			if(doEmpty)
			{
				_backingArray = new byte[origin._backingArray.Length];
			}
			else
			{
				_backingArray = new byte[origin._backingArray.Length];
				Buffer.BlockCopy(origin._backingArray, 0, _backingArray, 0, _backingArray.Length);
			}
		}

		public bool this[int x, int y]
		{
			get
			{
				if((x < 0 || x >= _fieldWidth) || (y < 0 || y >= _fieldHight))
					throw new IndexOutOfRangeException();

				//--> in range <--

				int bit = y * _fieldWidth + x;
				return (_backingArray[bit / 8] & (1 << (bit % 8))) != 0;
			}
			set
			{
				if((x < 0 || x >= _fieldWidth) || (y < 0 || y >= _fieldHight))
					throw new IndexOutOfRangeException();

				//--> in range <--

				int bit = y * _fieldWidth + x;
				if(value)
				{
					_backingArray[bit / 8] = (byte)(_backingArray[bit / 8] | (byte)(1 << (bit % 8)));
				}
				else
				{
					_backingArray[bit / 8] = (byte)(_backingArray[bit / 8] & ~(byte)(1 << (bit % 8)));
				}
			}
		}

		public Vector2Int GetSize()
		{
			return new Vector2Int(_fieldWidth, _fieldHight);
		}

		public byte[] ToArray()
		{
			return _backingArray;
		}

		public void FromArray(byte[] value)
		{
			if(value.Length != _backingArray.Length)
				throw new ArgumentException();

			//--> same array length <--

			_backingArray = value;
		}

		public static BitField2D operator +(BitField2D lhs, BitField2D rhs)
		{
			if(lhs._fieldWidth != rhs._fieldWidth || lhs._fieldHight != rhs._fieldHight)
				throw new ArgumentException();

			//--> same field structure <--

			byte[] tmp = new byte[lhs._backingArray.Length];
			for(int i = 0; i < lhs._backingArray.Length; i++)
			{
				tmp[i] = (byte)(lhs._backingArray[i] | rhs._backingArray[i]);
			}

			return new BitField2D(lhs._fieldWidth, lhs._fieldHight, tmp);
		}

		public static BitField2D operator -(BitField2D lhs, BitField2D rhs)
		{
			if(lhs._fieldWidth != rhs._fieldWidth || lhs._fieldHight != rhs._fieldHight)
				throw new ArgumentException();

			//--> same field structure <--

			byte[] tmp = new byte[lhs._backingArray.Length];
			for(int i = 0; i < lhs._backingArray.Length; i++)
			{
				tmp[i] = (byte)(lhs._backingArray[i] & ~rhs._backingArray[i]);
			}

			return new BitField2D(lhs._fieldWidth, lhs._fieldHight, tmp);
		}

		public Vector2Int[] GetActiveBits()
		{
			Profiler.BeginSample("[BitField] GetActiveBits");
			List<Vector2Int> value = new List<Vector2Int>();

			for(int y = 0; y < _fieldHight; y++)
			{
				for(int x = 0; x < _fieldWidth; x++)
				{
					if(this[x, y])
						value.Add(new Vector2Int(x, y));
				}
			}

			Profiler.EndSample();
			return value.ToArray();
		}

		public Vector2[] GetInactiveBits()
		{
			List<Vector2> value = new List<Vector2>();

			for(int y = 0; y < _fieldHight; y++)
			{
				for(int x = 0; x < _fieldWidth; x++)
				{
					if(!this[x, y])
						value.Add(new Vector2(x, y));
				}
			}

			return value.ToArray();
		}

		public bool AreAllBytesActive()
		{
			int completeBytes = Mathf.FloorToInt((_fieldWidth * _fieldHight) / 8.0f);
			for(int i = 0; i < completeBytes; i++)
			{
				if(_backingArray[i] != byte.MaxValue)
					return false;
			}
			if(_backingArray.Length > completeBytes)
			{
				int overhang = (_fieldWidth * _fieldHight) % 8;
				// (1 << overhang) == 2^overhang
				// (1 << overhang) - 1 for overhang is 3 == bx00000111
				// befor << (8 - overhang) for overhang is 3 == bx11100000
				if(_backingArray[_backingArray.Length - 1] < ((1 << overhang) - 1) << (8 - overhang))
					return false;
			}

			return true;
		}

		public bool AreAllByteInactive()
		{
			foreach(var it in _backingArray)
			{
				//requiers that overhang bits are zeroed
				if(it != byte.MinValue)
					return false;
			}

			return true;
		}
	}
}