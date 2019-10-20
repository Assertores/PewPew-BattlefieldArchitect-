using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BitField2D {
	byte[] m_backingArray = null;
	int m_fieldWidth = -1;

	public BitField2D(int width, int hight) {
		m_backingArray = new byte[Mathf.CeilToInt((width * hight) / 8.0f)];
		m_fieldWidth = width;
	}

	public BitField2D(int width, int hight, byte[] values) {
		m_backingArray = new byte[Mathf.CeilToInt((width * hight) / 8.0f)];
		m_fieldWidth = width;
		Buffer.BlockCopy(values, 0, m_backingArray, 0, Mathf.CeilToInt((width * hight) / 8.0f));
	}

	public bool this[int x, int y] {
		get {
			if ((x < 0 || x >= m_fieldWidth) || (y < 0 || y >= (m_backingArray.Length * 8) / m_fieldWidth))
				throw new System.IndexOutOfRangeException();

			int bit = y * m_fieldWidth + x;
			return (m_backingArray[bit / 8] & (1 << (bit % 8))) != 0;
		}
		set {
			if ((x < 0 || x >= m_fieldWidth) || (y < 0 || y >= (m_backingArray.Length * 8) / m_fieldWidth))
				throw new System.IndexOutOfRangeException();

			int bit = y * m_fieldWidth + x;
			if (value) {
				m_backingArray[bit / 8] = (byte)(m_backingArray[bit / 8] | (byte)(1 << (bit % 8)));
			} else {
				m_backingArray[bit / 8] = (byte)(m_backingArray[bit / 8] & ~(byte)(1 << (bit % 8)));
			}
		}
	}

	public byte[] ToArray() {
		return m_backingArray;
	}
}
