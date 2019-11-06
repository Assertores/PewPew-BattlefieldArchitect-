using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class Building : MonoBehaviour
	{
		public static List<Building> s_refs { get; private set; } = new List<Building>();

		private void Start()
		{
			s_refs.Add(this);
		}

		private void OnDestroy()
		{
			s_refs.Remove(this);
		}

		public bool Select(Rect aabb)
		{
			Vector2 pos = Camera.main.WorldToScreenPoint(transform.position);

			if(pos.x > aabb.xMin && pos.x < aabb.xMax && pos.y > aabb.yMin && pos.y < aabb.yMax)
			{
				print(gameObject.name);
				return true;
			}

			return false;
		}
	}
}
