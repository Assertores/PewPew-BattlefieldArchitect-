using System.Collections;
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
		private List<MonoBehaviour> _elements = new List<MonoBehaviour>();
		private System.Type _type;
		private Transform _parent;
		private int _stepSize;
		private ObjectType _oType = ObjectType.SIZE;

		ObjectPool(GameObject prefab, int initialSize, Transform parent, System.Type type, ObjectType objectType)
		{
			_prefab = prefab;
			_parent = parent;
			_stepSize = initialSize;
			_oType = objectType;
			_type = type;

			Resize().gameObject.SetActive(false);
		}

		/// <summary>
		/// creates an objectPool with an prefab as key
		/// </summary>
		/// <typeparam name="T">the type as MonoBehaviour, the prefab hast to have as component on the top level</typeparam>
		/// <param name="prefab">the prefab, that should be used for the object pool</param>
		/// <param name="initialSize">the size of the object pool</param>
		/// <param name="grandParent">the parent object in witch the objectpool will initialice in objectholder in witch the objects will be instanciated into</param>
		/// <returns>the objectpool of the prefab type. null if prefab is null, prefab has not T component at the top level, or the prefab has no INetElement on any level if it is flaged as doTrackInNetwork</returns>
		public static ObjectPool CreatePool<T>(GameObject prefab, int initialSize, Transform grandParent) where T : MonoBehaviour
		{
			if(prefab == null)
				return null;
			if(s_objectPools.ContainsKey(prefab))
				return s_objectPools[prefab];
			if(!prefab.GetComponent(typeof(T)))
				return null;

			GameObject tmp = new GameObject(prefab.name);
			tmp.transform.parent = grandParent;
			s_objectPools[prefab] = new ObjectPool(prefab, initialSize, tmp.transform, typeof(T), ObjectType.SIZE);
			return s_objectPools[prefab];
		}

		public static ObjectPool CreatePool<T>(ObjectType type, int initialSize, Transform grandParent) where T : MonoBehaviour
		{
			if(type == ObjectType.SIZE)
			{
				Debug.LogError("type is invalide");
				return null;
			}

			GameObject prefab = GlobalVariables.s_instance._prefabs[(int)type];
			if(prefab == null)
			{
				Debug.LogError("prefab not found");
				return null;
			}
			if(s_objectPools.ContainsKey(prefab))
			{
				Debug.LogError("object pool already exists");
				return s_objectPools[prefab];
			}
			if(!prefab.GetComponent(typeof(T)))
			{
				Debug.LogError("skript is not on the gameobject: " + prefab.name);
				return null;
			}
			if(prefab.GetComponentsInChildren<INetElement>().Length <= 0)
			{
				Debug.LogError("prefab has no INetElement Components");
				return null;
			}

			GameObject tmp = new GameObject(prefab.name);
			tmp.transform.parent = grandParent;
			s_objectPools[prefab] = new ObjectPool(prefab, initialSize, tmp.transform, typeof(T), type);
			return s_objectPools[prefab];
		}

		/// <summary>
		/// use this to get a free element in the object pool whitch will be automaticly already be set active.
		/// </summary>
		/// <param name="team">the team that should be aplyed to the IRefHolder if set</param>
		/// <returns>the reference to the type as MonoBehaviour</returns>
		public MonoBehaviour GetNextObject(int team = -1)
		{

			for(int i = 0; i < _parent.childCount; i++)
			{
				if(!_parent.GetChild(i).gameObject.activeSelf)
				{
					if(team >= 0 && _elements[i] is IRefHolder)
						(_elements[i] as IRefHolder)._team = team;

					_parent.GetChild(i).gameObject.SetActive(true);
					return _elements[i];
				}
			}

			return Resize();
		}

		/// <summary>
		/// use this to safly cast the MonoBehaviour if you don't know the corect type;
		/// </summary>
		/// <returns>the type into with the MonoBehaviour can be casted</returns>
		public System.Type GetReferenceType()
		{
			return _type;
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

		public MonoBehaviour Resize(int range = -1, int startID = -1)
		{
			if(_oType != ObjectType.SIZE && startID < 0)
			{
				startID = GameNetcode.s_instance.GetNewIDRange(_oType, _stepSize);
			}

			GameObject firstElement = GameObject.Instantiate(_prefab, _parent);
			firstElement.name = _prefab.name + " (" + _parent.childCount + ")";

			if(_oType != ObjectType.SIZE)
			{
				foreach(var it in firstElement.GetComponentsInChildren<INetElement>())
				{
					it._id = startID;
				}
				startID++;
			}
			MonoBehaviour value = firstElement.GetComponent(_type) as MonoBehaviour;
			_elements.Add(value);

			if(range > 0)
				_stepSize = range;

			for(int i = 1; i < _stepSize; i++)
			{
				GameObject tmp = GameObject.Instantiate(_prefab, _parent);
				tmp.SetActive(false);
				tmp.name = _prefab.name + " (" + _parent.childCount + ")";

				if(_oType != ObjectType.SIZE)
				{
					foreach(var it in tmp.GetComponentsInChildren<INetElement>())
					{
						it._id = startID;
					}
					startID++;
				}
				MonoBehaviour script = tmp.GetComponent(_type) as MonoBehaviour;
				_elements.Add(script);
			}

			return value;
		}
	}
}
