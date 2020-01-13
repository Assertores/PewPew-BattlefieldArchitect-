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
		ResourceMapCalculate.s_instance.AddFabric(holder);

	}

	// Update is called once per frame
	void Update()
    {
		if(Input.GetKeyDown(KeyCode.X))
		{
			ResourceMapCalculate.s_instance.StartCalculation();
		}
    }
}
