using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{

	private static T s_instanceBackingField = default;
	public static T s_instance
	{
		get
		{
			if(s_instanceBackingField == null)
			{
				Debug.Log("no object of type " + typeof(T) + " was found.");
				GameObject tmp = new GameObject("SINGELTON_" + typeof(T));
				s_instanceBackingField = tmp.AddComponent<T>();
			}
			return s_instanceBackingField;
		}
	}

	private void Awake()
	{
		if(s_instanceBackingField != null)
		{
			Debug.Log("multible instances of type " + typeof(T));
			Destroy(this);
			return;
		}
		s_instanceBackingField = this as T;
	}

	public static bool Exists()
	{
		return s_instanceBackingField != default;
	}
}