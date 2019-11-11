﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class ObjectPool
	{
		/// <summary>
		/// CreatePool(PREFAB, SIZE, PARENT) 
		/// s_objectPools[PREFAB].GetNextObject(); => Gives you an available object of the objectpool, and will turn it on. If no one is available, the object pool will resize.
		/// FreeObject(ELEMENT) will return the element back to the pool, but the same is achieved by deactivating the gameobject
		/// </summary>
		public static Dictionary<GameObject, ObjectPool> s_objectPools { get; private set; } = new Dictionary<GameObject, ObjectPool>();

		private GameObject _prefab;
		private Transform _parent;
		private int _stepSize;

		ObjectPool(GameObject prefab, int initialSize, Transform parent)
		{
			_prefab = prefab;
			_parent = parent;
			_stepSize = initialSize;

			Resize().SetActive(false);
		}

		public static ObjectPool CreatePool(GameObject prefab, int initialSize, Transform grandParent)
		{
			if(prefab == null)
				return null;
			if(s_objectPools.ContainsKey(prefab))
				return s_objectPools[prefab];

			GameObject tmp = new GameObject(prefab.name);
			tmp.transform.parent = grandParent;
			s_objectPools[prefab] = new ObjectPool(prefab, initialSize, tmp.transform);
			return s_objectPools[prefab];
		}

		public GameObject GetNextObject()
		{

			for(int i = 0; i < _parent.childCount; i++)
			{
				if(!_parent.GetChild(i).gameObject.activeSelf)
				{
					_parent.GetChild(i).gameObject.SetActive(true);
					return _parent.GetChild(i).gameObject;
				}
			}

			return Resize();
		}

		public void FreeObject(GameObject element)
		{
			for(int i = 0; i < _parent.childCount; i++)
			{
				if(_parent.GetChild(i).gameObject == element)
				{
					element.SetActive(false);
					return;
				}
			}
		}

		private GameObject Resize()
		{
			GameObject value = GameObject.Instantiate(_prefab, _parent);
			value.name = _prefab.name + " (" + _parent.childCount + ")";

			for(int i = 1; i < _stepSize; i++)
			{
				GameObject tmp = GameObject.Instantiate(_prefab, _parent);
				tmp.SetActive(false);
				tmp.name = _prefab.name + " (" + _parent.childCount + ")";
			}

			return value;
		}
	}
}
