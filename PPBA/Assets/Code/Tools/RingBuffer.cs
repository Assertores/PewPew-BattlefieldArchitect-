using UnityEngine;

namespace PPBA
{
	[System.Serializable]
	public class RingBuffer<T>
	{
		[SerializeField] private T[] _backingArray = new T[4];//backing array of elements
		[SerializeField] private int _lowPos = 0;//position of the element with the lowest index in the backing array
		[SerializeField] private int _lowend = 0;//the lowest index valide
		[SerializeField] private int _highend = 0;//the highest index valide

		int Index(int value)
		{
			return Lib.Mod((value - _lowend + _lowPos), _backingArray.Length);
		}

		public ref T this[int key]
		{
			get //for reference values getter will be called by setting the value
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
					//int elementSize = Marshal.SizeOf(typeof(T));//sizeof(tmp[0]);//System.Buffer.ByteLength(_backingArray) / _backingArray.Length;
					//System.Buffer.BlockCopy(_backingArray, _lowPos * elementSize, tmp, 0, firstBlock * elementSize);
					for(int i = 0; i < firstBlock; i++)
					{
						tmp[i] = _backingArray[_lowPos + i];
					}
					//System.Buffer.BlockCopy(_backingArray, 0, tmp, firstBlock * elementSize, _lowPos * elementSize);
					for(int i = 0; i < _lowPos; i++)
					{
						tmp[firstBlock + i] = _backingArray[i];
					}
					_backingArray = tmp;
					_lowPos = 0;
				}

				return ref _backingArray[Index(key)];
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
				T element = _backingArray[Index(i)];
				sb.AppendLine(i + ": " + (element == null ? "NULL" : element.ToString()));
			}

			sb.AppendLine("===== ===== Memory Array ===== =====");
			for(int i = 0; i < _backingArray.Length; i++)
			{
				T element = _backingArray[i];
				sb.AppendLine(i + ": " + (element == null ? "NULL" : element.ToString()));
			}

			return sb.ToString();
		}
	}

	[System.Serializable]
	public class RingBuffer_GameState : RingBuffer<GameState> { }

	[System.Serializable]
	public class RingBuffer_InputState : RingBuffer<InputState> { }
}
