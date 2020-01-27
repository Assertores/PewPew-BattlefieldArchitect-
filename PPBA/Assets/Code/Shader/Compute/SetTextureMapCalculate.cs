using UnityEngine;

namespace PPBA
{
	class SetTextureMapCalculate : MonoBehaviour
	{
		public ComputeShader _computeShader;

		private ComputeBuffer _redValue;
		private ComputeBuffer _terValue;

		public int _resourceCalcKernel;

		//public SetTextureMapCalculate(int kernel)
		//{
		//	_resourceCalcKernel = kernel;
		//}

		public float[] bub;

		public void StartComputeShader(float[] ResTex, float[] TerTex, RenderTexture ResRenTex, RenderTexture TerRenTex)
		{
			_redValue = new ComputeBuffer( (256 * 256), sizeof(float));
			_terValue = new ComputeBuffer( (256 * 256), sizeof(float));

			_redValue.SetData(ResTex);
			_terValue.SetData(TerTex);
			bub = ResTex;

			_computeShader.SetBuffer(_resourceCalcKernel, "InputRedValue", _redValue);
			_computeShader.SetBuffer(_resourceCalcKernel, "InputTextureTerritorium", _terValue);
			
   
			_computeShader.SetTexture(_resourceCalcKernel, "Result", ResRenTex);
			_computeShader.SetTexture(_resourceCalcKernel, "TerritoriumResult", TerRenTex);

			// Start ComputeShader
			_computeShader.Dispatch(_resourceCalcKernel, 256 / 16, 256 / 16, 1);

			_redValue.Release();
			_terValue.Release();
		}
	}
}
