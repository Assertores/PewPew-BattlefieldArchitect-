using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class RingBuffer<T>
	{
		private T[] _backingArray = new T[4];//backing array of elements
		private int _lowPos = 0;//position of the element with the lowest index in the backing array
		private int _lowend = 0;//the lowest index valide
		private int _highend = 0;//the highest index valide

		int Index(int value)
		{
			return Lib.Mod((value - _lowend + _lowPos), _backingArray.Length);
		}

		public T this[int key]
		{
			get
			{
				if(key < _lowend || key > _highend)
				{
					throw new System.IndexOutOfRangeException();
				}

				return _backingArray[Index(key)];
			}
			set
			{
				if(key < _lowend)
				{
					_lowPos -= _lowend - key;
					_lowPos = Lib.Mod(_lowPos, _backingArray.Length);
					_lowend = key;
				}
				else if(key > _highend)
				{
					_highend = key;
				}
				if(_highend - _lowend >= _backingArray.Length)
				{
					T[] tmp = new T[Mathf.NextPowerOfTwo(_highend - _lowend + 1)];
					int firstBlock = _backingArray.Length - _lowPos;
					int elementSize = System.Buffer.ByteLength(_backingArray) / _backingArray.Length;
					System.Buffer.BlockCopy(_backingArray, _lowPos * elementSize, tmp, 0, firstBlock * elementSize);
					System.Buffer.BlockCopy(_backingArray, 0, tmp, firstBlock * elementSize, _lowPos * elementSize);
					_backingArray = tmp;
					_lowPos = 0;
				}

				_backingArray[Index(key)] = value;
			}
		}

		public int GetLowEnd()
		{
			return _lowend;
		}

		public int GetHighEnd()
		{
			return _highend;
		}

		/// <summary>
		/// not including
		/// </summary>
		public void FreeUpTo(int key)
		{
			if(key <= _lowend)
				return;

			ClearRange(_lowend, key);
			_lowPos += key - _lowend;
			_lowPos = Lib.Mod(_lowPos, _backingArray.Length);
			_lowend = key;
		}

		/// <summary>
		/// not including
		/// </summary>
		public void FreeDownTo(int key)
		{
			if(key >= _highend)
				return;

			ClearRange(key + 1, _highend + 1);
			_highend = key;
		}

		/// <summary>
		/// clears a range and takes care of looping around
		/// </summary>
		/// <param name="start">includet</param>
		/// <param name="end">excludet</param>
		public void ClearRange(int start, int end)
		{
			if(start < _lowend || end > _highend + 1)
				throw new System.IndexOutOfRangeException();

			start = Index(start);
			end = Index(end);
			if(start <= end)
			{
				System.Array.Clear(_backingArray, start, end - start);
			}
			else
			{
				System.Array.Clear(_backingArray, 0, end);
				System.Array.Clear(_backingArray, start, _backingArray.Length - start);
			}
		}

		public override string ToString()
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();

			sb.AppendLine("===== ===== Logic Array ===== =====");
			for(int i = _lowend; i <= _highend; i++)
			{
				sb.AppendLine(i + ": " + _backingArray[Index(i)].ToString());
			}

			sb.AppendLine("===== ===== Memory Array ===== =====");
			for(int i = 0; i < _backingArray.Length; i++)
			{
				sb.AppendLine(i + ": " + _backingArray[i].ToString());
			}

			return sb.ToString();
		}
	}
}
