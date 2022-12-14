using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PPBA;

public class TestCalc : MonoBehaviour
{
#if UNITY_EDITOR
	public Transform test2;

	// Start is called before the first frame update
	void Start()
	{
		IRefHolder holder = GetComponent<IRefHolder>();
		HeatMapCalcRoutine.s_instance.AddFabric(holder);

		HeatMapCalcRoutine.s_instance.AddFabric(test2.GetComponent<IRefHolder>());
	}

	// Update is called once per frame
	void Update()
	{
		if(Input.GetKeyDown(KeyCode.X))
		{
			StartCoroutine(StartRoutine());
			//HeatMapCalcRoutine.s_instance.EarlyCalc();
			print("liste " + HeatMapCalcRoutine.s_instance._Refinerys.Count);
		}
	}

	public float[] test;
	IEnumerator StartRoutine()
	{
		HeatMapCalcRoutine.s_instance.StartHeatMapCalc();
		yield return new WaitForSeconds(0.1f);

		HeatMapReturnValue[] holder = new HeatMapReturnValue[2];

		holder = HeatMapCalcRoutine.s_instance.ReturnValue();
		test = holder[1].tex;
		HeatMapCalcRoutine.s_instance.SetRendererTextures(holder[0].tex, holder[1].tex);
		yield return null;
	}
#endif
}
