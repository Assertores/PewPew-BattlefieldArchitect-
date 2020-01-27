using UnityEngine;

public class EarlyCalculate
{
	public ComputeShader _computeShader;

	ComputeBuffer _bitField;
	ComputeBuffer _buffer;
	ComputeBuffer _RedValueBuffer;

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

		_computeShader.SetBuffer(_resourceCalcKernel, "buffer", _buffer);
		_computeShader.SetBuffer(_resourceCalcKernel, "redValue", _RedValueBuffer);
		_computeShader.SetBuffer(_resourceCalcKernel, "bitField", _bitField);

		_computeShader.Dispatch(_resourceCalcKernel, 256 / 16, 256 / 16, 1); // prüfen ob er hier wartet

	}
}
