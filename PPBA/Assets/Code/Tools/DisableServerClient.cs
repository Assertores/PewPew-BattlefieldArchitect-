using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class DisableServerClient : MonoBehaviour
	{

		[SerializeField] bool keapOnClient = true;

		private void Awake()
		{
#if UNITY_SERVER
			if(keapOnClient)
				gameObject.SetActive(false);
			else
				gameObject.SetActive(true);
#else
			if(!keapOnClient)
				gameObject.SetActive(false);
			else
				gameObject.SetActive(true);
#endif
		}
	}
}