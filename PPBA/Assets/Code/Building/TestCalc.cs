using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PPBA;

public class TestCalc : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
		IRefHolder holder = GetComponent<IRefHolder>();
		HeatMapCalcRoutine.s_instance.AddFabric(holder);
	}

	// Update is called once per frame
	void Update()
    {
		if(Input.GetKeyDown(KeyCode.X))
		{
			HeatMapCalcRoutine.s_instance.StartHeatMapCalc();
			print("liste " + HeatMapCalcRoutine.s_instance._Refinerys.Count);
		}
    }
}
