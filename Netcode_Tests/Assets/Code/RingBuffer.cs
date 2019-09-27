using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RingBuffer<T> {

	T[] m_backingArray = new T[4];//backing array of elements
	int m_lowPos = 0;//position of the element with the lowest index in the backing array
	int m_lowend = 0;//the lowest index valide
	int m_highend = 0;//the highest index valide

	int index(int value) {
		return Lib.mod((value - m_lowend + m_lowPos), m_backingArray.Length);
	}

	public T this[int key] {
		get {
			if (key < m_lowend || key > m_highend) {
				throw new System.IndexOutOfRangeException();
			}
			return m_backingArray[index(key)];
		}
		set {
			if (key < m_lowend) {
				m_lowPos -= m_lowend - key;
				m_lowPos = Lib.mod(m_lowPos, m_backingArray.Length);
				m_lowend = key;
			}else if(key > m_highend) {
				m_highend = key;
				
			}
			if(m_highend-m_lowend >= m_backingArray.Length) {
				T[] tmp = new T[Mathf.NextPowerOfTwo(m_highend - m_lowend + 1)];
				int firstBlock = m_backingArray.Length - m_lowPos;
				int elementSize = System.Buffer.ByteLength(m_backingArray)/m_backingArray.Length;
				System.Buffer.BlockCopy(m_backingArray, m_lowPos * elementSize, tmp, 0, firstBlock * elementSize);
				System.Buffer.BlockCopy(m_backingArray, 0, tmp, firstBlock * elementSize, m_lowPos * elementSize);
				m_backingArray = tmp;
				m_lowPos = 0;
			}

			m_backingArray[index(key)] = value;
		}
	}

	public int GetLowEnd() {
		return m_lowend;
	}

	public int GetHighEnd() {
		return m_highend;
	}

	/// <summary>
	/// not including
	/// </summary>
	public void FreeUpTo(int key) {
		if (key <= m_lowend)
			return;

		m_lowPos += key - m_lowend;
		m_lowPos = Lib.mod(m_lowPos, m_backingArray.Length);
		m_lowend = key;
	}

	/// <summary>
	/// not including
	/// </summary>
	public void FreeDownTo(int key) {
		if (key >= m_highend)
			return;

		m_highend = key;
	}

	public override string ToString() {
		System.Text.StringBuilder sb = new System.Text.StringBuilder();

		sb.AppendLine("===== ===== Logic Array ===== =====");
		for (int i = m_lowend; i <= m_highend; i++) {
			sb.AppendLine(i + ": " + m_backingArray[index(i)].ToString());
		}
		sb.AppendLine("===== ===== Memory Array ===== =====");
		for (int i = 0; i < m_backingArray.Length; i++) {
			sb.AppendLine(i + ": " + m_backingArray[i].ToString());
		}
		return sb.ToString();
	}
}
