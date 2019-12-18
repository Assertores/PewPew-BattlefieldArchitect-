using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PPBA;
using System.Text;
using System;

public class TestBitField2D : MonoBehaviour
{
	[TextArea] [SerializeField] string Output;
	// Start is called before the first frame update
	void Start()
	{
		StringBuilder value = new StringBuilder();
		StringBuilder line = new StringBuilder();

		BitField2D subject1 = new BitField2D(5, 5);
		subject1[0, 0] = true;
		subject1[2, 4] = true;
		BitField2D subject2 = new BitField2D(5, 5, subject1.ToArray());
		subject1[1, 0] = true;
		subject2[4, 4] = true;
		{
			value.AppendLine("===== ===== A ===== =====");
			value.AppendLine(subject1.GetSize().ToString());
			{
				line.Clear();
				byte[] info = subject1.ToArray();
				for(int i = 0; i < info.Length; i++)
					line.Append(Convert.ToString(info[i], 2).PadLeft(8, '0') + " ");
				value.AppendLine(line.ToString());
			}
			{
				line.Clear();
				Vector2Int[] info = subject1.GetActiveBits();
				for(int i = 0; i < info.Length; i++)
					line.Append(info[i].ToString() + ", ");
				value.AppendLine(line.ToString());
			}
			value.AppendLine(subject2.GetSize().ToString());
			{
				line.Clear();
				byte[] info = subject2.ToArray();
				for(int i = 0; i < info.Length; i++)
					line.Append(Convert.ToString(info[i], 2).PadLeft(8, '0') + " ");
				value.AppendLine(line.ToString());
			}
			{
				line.Clear();
				Vector2Int[] info = subject2.GetActiveBits();
				for(int i = 0; i < info.Length; i++)
					line.Append(info[i].ToString() + ", ");
				value.AppendLine(line.ToString());
			}
		}
		subject1[0, 1] = true;
		subject1[0, 2] = true;
		subject1[0, 3] = true;
		subject1[0, 4] = true;
		subject1[1, 0] = true;
		subject1[1, 1] = true;
		subject1[1, 2] = true;
		subject1[1, 3] = true;
		subject1[1, 4] = true;
		subject1[2, 0] = true;
		subject1[2, 1] = true;
		subject1[2, 2] = true;
		subject1[2, 3] = true;
		subject1[2, 4] = true;
		subject1[3, 0] = true;
		subject1[3, 1] = true;
		subject1[3, 2] = true;
		subject1[3, 3] = true;
		subject1[3, 4] = true;
		subject1[4, 0] = true;
		subject1[4, 1] = true;
		subject1[4, 2] = true;
		subject1[4, 3] = true;
		subject1[4, 4] = true;

		BitField2D subject3 = new BitField2D(subject1, true);

		{
			value.AppendLine("===== ===== B ===== =====");
			{
				line.Clear();
				byte[] info = subject1.ToArray();
				for(int i = 0; i < info.Length; i++)
					line.Append(Convert.ToString(info[i], 2).PadLeft(8, '0') + " ");
				value.AppendLine(line.ToString());
			}
			value.AppendLine(subject1.AreAllBytesActive() + ", " + subject1.AreAllByteInactive());
			{
				line.Clear();
				byte[] info = subject2.ToArray();
				for(int i = 0; i < info.Length; i++)
					line.Append(Convert.ToString(info[i], 2).PadLeft(8, '0') + " ");
				value.AppendLine(line.ToString());
			}
			value.AppendLine(subject2.AreAllBytesActive() + ", " + subject2.AreAllByteInactive());
			{
				line.Clear();
				byte[] info = subject3.ToArray();
				for(int i = 0; i < info.Length; i++)
					line.Append(Convert.ToString(info[i], 2).PadLeft(8, '0') + " ");
				value.AppendLine(line.ToString());
			}
			value.AppendLine(subject3.AreAllBytesActive() + ", " + subject3.AreAllByteInactive());
		}

		Output = value.ToString();
	}
}
