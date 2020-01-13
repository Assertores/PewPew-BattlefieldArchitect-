using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using PPBA;
using System;

public class TestComputeShader : MonoBehaviour
{
	public ComputeShader _computeShader;

	[TextArea] [SerializeField] string output;
	void Start()
	{
		StringBuilder value = new StringBuilder();

		BitField2D subject = new BitField2D(8, 8);

		byte[] field = subject.ToArray();

		RenderTexture tex = new RenderTexture(8, 8, 0, RenderTextureFormat.R8)
		{
			enableRandomWrite = true
		};
		tex.Create();

		//field im computeshader setzten
		int _resourceCalcKernel1 = _computeShader.FindKernel("CSMain");
		ComputeBuffer _bitField = new ComputeBuffer(((8 * 8) / 8 / sizeof(int)), sizeof(int));
		
		int _resourceCalcKernel2 = _computeShader.FindKernel("CSInit");

		_computeShader.SetBuffer(_resourceCalcKernel2, "bitField", _bitField);

		_computeShader.SetBuffer(_resourceCalcKernel1, "bitField", _bitField);
		_computeShader.SetTexture(_resourceCalcKernel1, "Result", tex);

		//computeshader ausführen
		_computeShader.Dispatch(_resourceCalcKernel2, 50, 1, 1);
		_computeShader.Dispatch(_resourceCalcKernel1, 1, 1, 1);

		//field retreaven

		_bitField.GetData(field);
		_bitField.Release();

		for(int i = 0; i < field.Length; i++)
		{
			value.Append(Convert.ToString(field[i], 2).PadLeft(8, '0') + " ");
		}
		value.AppendLine();

		subject.FromArray(field);

		Vector2Int[] pos = subject.GetActiveBits();
		for(int i = 0; i < pos.Length; i++)
		{
			value.Append(pos[i].ToString() + ", ");
		}

		output = value.ToString();
	}
}
