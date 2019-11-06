using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA {
	public class SelectInput : MonoBehaviour
	{
		private Vector2 _starPos;
		void Update()
		{
			if(Input.GetMouseButtonDown(0))
			{
				_starPos = Input.mousePosition;
			} else if(Input.GetMouseButtonUp(0))
			{
				Rect aabb = new Rect();
				aabb.xMin = Mathf.Min(_starPos.x, Input.mousePosition.x);
				aabb.xMax = Mathf.Max(_starPos.x, Input.mousePosition.x);
				aabb.yMin = Mathf.Min(_starPos.y, Input.mousePosition.y);
				aabb.yMax = Mathf.Max(_starPos.y, Input.mousePosition.y);

				foreach(var it in Building.s_refs)
				{
					it.Select(aabb);
				}
			}
		}
	}
}
