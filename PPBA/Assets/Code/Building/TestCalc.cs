﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PPBA;

public class TestCalc : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		if(Input.GetKeyDown(KeyCode.Y))
		{
			ResourceMapCalculate.s_instance.RefreshCalcRes();
		}
		
			

		
    }
}