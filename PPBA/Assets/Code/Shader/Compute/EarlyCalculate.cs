using UnityEngine;

public class EarlyCalculate
{
	public ComputeShader _computeShader;

	ComputeBuffer _bitField;
	ComputeBuffer _buffer;
	ComputeBuffer _RedValueBuffer;
	ComputeBuffer _InputRedValue;
	ComputeBuffer _InputTextureTerritorium;

	int _resourceCalcKernel;

	public EarlyCalculate(int kernel)
	{
		_resourceCalcKernel = kernel;
	}

	public void EarlyCalulation(int RefineryCount)
	{
		_bitField = new ComputeBuffer(((256 * 256) / 8 / sizeof(int)), sizeof(int));
		_buffer = new ComputeBuffer(RefineryCount, sizeof(int));
		_RedValueBuffer = new ComputeBuffer((256 * 256), sizeof(float));
		_InputRedValue = new ComputeBuffer((256 * 256), sizeof(float));
		_InputTextureTerritorium = new ComputeBuffer((256 * 256), sizeof(float));

		_computeShader.SetBuffer(_resourceCalcKernel, "buffer", _buffer);
		_computeShader.SetBuffer(_resourceCalcKernel, "redValue", _RedValueBuffer);
		_computeShader.SetBuffer(_resourceCalcKernel, "bitField", _bitField);
		_computeShader.SetBuffer(_resourceCalcKernel, "InputRedValue", _InputRedValue);
		_computeShader.SetBuffer(_resourceCalcKernel, "InputTextureTerritorium", _InputTextureTerritorium);

		_computeShader.Dispatch(_resourceCalcKernel, 256 / 16, 256 / 16, 1); // prüfen ob er hier wartet

		_buffer.Release();
		_RedValueBuffer.Release();
		_bitField.Release();
		_InputRedValue.Release();
		_InputTextureTerritorium.Release();
	}
}
