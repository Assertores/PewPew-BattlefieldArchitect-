using UnityEngine;

namespace PPBA
{
	class SetTextureMapCalculate
	{
		public ComputeShader _computeShader;
		public int _resourceCalcKernel;

		private ComputeBuffer _redValue;
		private ComputeBuffer _terValue;

		public Vector4[] colorVecs;

		public SetTextureMapCalculate(int kernel, Color[] colors)
		{
			_resourceCalcKernel = kernel;
			colorVecs = colors.ColorToVector4();
		}

		public void StartComputeShader(float[] ResTex, float[] TerTex, RenderTexture ResRenTex, RenderTexture TerRenTex, RenderTexture MyTerRenTex)
		{
			_redValue = new ComputeBuffer( (256 * 256), sizeof(float));
			_terValue = new ComputeBuffer( (256 * 256), sizeof(float));

			_redValue.SetData(ResTex);
			_terValue.SetData(TerTex);


			_computeShader.SetBuffer(_resourceCalcKernel, "InputRedValue", _redValue);
			_computeShader.SetBuffer(_resourceCalcKernel, "terValue", _terValue);
			_computeShader.SetVectorArray("teamColors", colorVecs);
			_computeShader.SetInt("teamColors", GlobalVariables.s_instance._clients[0]._id);

			Debug.Log("team in compute setTer shader : " + GlobalVariables.s_instance._clients[0]._id);

			_computeShader.SetTexture(_resourceCalcKernel, "Result", ResRenTex);
			_computeShader.SetTexture(_resourceCalcKernel, "TerritoriumResult", TerRenTex);
			_computeShader.SetTexture(_resourceCalcKernel, "MyTerritoriumResult", MyTerRenTex);

			// Start ComputeShader
			_computeShader.Dispatch(_resourceCalcKernel, 256 / 8, 256 / 8, 1);

			_redValue.Release();
			_terValue.Release();

		}
	}
}
