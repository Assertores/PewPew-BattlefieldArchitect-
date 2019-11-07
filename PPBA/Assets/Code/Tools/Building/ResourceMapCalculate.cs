using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class ResourceMapCalculate : Singleton<ResourceMapCalculate>
	{
		public ComputeShader _computeShader;
		public Renderer _GroundRenderer;

		private RenderTexture _ResultTexture;
		private ComputeBuffer _buffer;
		private Texture2D _resourceTexture;

		private int resourceCalcKernel;
		private int resourceCalcKernel2;
		private List<Vector4> fabricCenter = new List<Vector4>();

		[SerializeField]
		private int[] CurrentValue;

		void Start()
		{
			_resourceTexture = Instantiate(_GroundRenderer.material.GetTexture("_NoiseMap")) as Texture2D;
			Texture2D resourceTexture = Instantiate(_GroundRenderer.material.GetTexture("_NoiseMap")) as Texture2D;

			resourceCalcKernel = _computeShader.FindKernel("CSMain");
			resourceCalcKernel2 = _computeShader.FindKernel("CSInit");

			_ResultTexture = new RenderTexture(resourceTexture.height, resourceTexture.width, 24)
			{
				enableRandomWrite = true
			};

			_ResultTexture.Create();
			Graphics.CopyTexture(resourceTexture, _ResultTexture);

		}

		public void AddFabric(Vector3 pos, float intensity, float radius)
		{
			fabricCenter.Add(new Vector4(pos.x, pos.z, intensity, radius));
		}

		private void RefreshCalcRes()
		{
			_buffer = new ComputeBuffer(50, sizeof(int));

			_computeShader.SetInt("PointSize", fabricCenter.Count);
			_computeShader.SetVectorArray("coords", fabricCenter.ToArray());

			_computeShader.SetBuffer(resourceCalcKernel2, "buffer", _buffer);
			_computeShader.SetBuffer(resourceCalcKernel, "buffer", _buffer);

			_computeShader.SetTexture(resourceCalcKernel, "Result", _ResultTexture);

			_computeShader.Dispatch(resourceCalcKernel2, 50, 1, 1); // prüfen ob er hier wartet
			_computeShader.Dispatch(resourceCalcKernel, 512 / 8, 512 / 8, 1);

			_buffer.GetData(CurrentValue);
			_buffer.Release();
			_buffer = null;
			_GroundRenderer.material.SetTexture("_NoiseMap", _ResultTexture);
			SendToTickManager();

		}

		private void SendToTickManager()
		{
		//	ressourceManager.AddRessourcesToRefineries(CurrentValue);
		}

		private bool _changeMap = false;

		public void SwitchMap()
		{
			_changeMap = !_changeMap;

			int t = _changeMap ? 0 : 1;
			_GroundRenderer.material.SetFloat("_MetalResourcesInt", t);
		}

	}

}

