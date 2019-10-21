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
				throw new IndexOutOfRangeException();

			int bit = y * m_fieldWidth + x;
			return (m_backingArray[bit / 8] & (1 << (bit % 8))) != 0;
		}
		set {
			if ((x < 0 || x >= m_fieldWidth) || (y < 0 || y >= (m_backingArray.Length * 8) / m_fieldWidth))
				throw new IndexOutOfRangeException();

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

	public void FromArray(byte[] value) {
		if (value.Length != m_backingArray.Length)
			throw new ArgumentException();

		m_backingArray = value;
	}

	public static BitField2D operator+ (BitField2D lhs, BitField2D rhs) {
		if (lhs.m_fieldWidth != rhs.m_fieldWidth || lhs.m_backingArray.Length != rhs.m_backingArray.Length)
			throw new ArgumentException();

		byte[] tmp = new byte[lhs.m_backingArray.Length];
		for (int i = 0; i < lhs.m_backingArray.Length; i++) {
			tmp[i] = (byte)(lhs.m_backingArray[i] | rhs.m_backingArray[i]);
		}

		return new BitField2D(lhs.m_fieldWidth, (lhs.m_backingArray.Length * 8) / lhs.m_fieldWidth, tmp);
	}

	public Vector2[] GetActiveBits() {
		List<Vector2> value = new List<Vector2>();

		for(int y = 0; y < (m_backingArray.Length * 8) / m_fieldWidth; y++) {
			for(int x = 0; x < m_fieldWidth; x++) {
				if (this[x, y])
					value.Add(new Vector2(x, y));
			}
		}

		return value.ToArray();
	}
}
