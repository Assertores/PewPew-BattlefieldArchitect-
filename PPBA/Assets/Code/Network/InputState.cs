using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public enum ObjectType { NON, REFINARY, WALL, SIZE}

	namespace ISC //Input State Component
	{
		public class isc
		{
			public int _id;
			public int _client;
			public ObjectType _type;
		}

		public class obj : isc
		{
			public Vector3 _pos;
			public float _angle; //in degrees
		}

		public class combinedObj : isc
		{
			public Vector3[] _corners;
		}
	}

	public class InputState : MonoBehaviour
	{
		private static int _currentID = 0;

		public List<ISC.obj> _objs = new List<ISC.obj>();
		public List<ISC.combinedObj> _combinedObjs = new List<ISC.combinedObj>();

		/// <summary>
		/// adds a new object to the InputState
		/// </summary>
		/// <param name="type">type of the placed object</param>
		/// <param name="pos">position of the placed object</param>
		/// <param name="angle">rotation of the placed object around the y axis</param>
		/// <returns>input identifyer</returns>
		public int AddObj(ObjectType type, Vector3 pos, float angle)
		{
			ISC.obj value = new ISC.obj();
			value._id = _currentID++;
			value._type = type;
			value._pos = pos;
			value._angle = angle;
			_objs.Add(value);
			return value._id;
		}

		/// <summary>
		/// adds a wall or streat or trench or comparable to the InputState
		/// </summary>
		/// <param name="type">type of the placed object</param>
		/// <param name="corners">list of corner positions</param>
		/// <returns>input identifyer</returns>
		public int AddCombinedObj(ObjectType type, List<Vector3> corners)
		{
			ISC.combinedObj value = new ISC.combinedObj();
			value._id = _currentID++;
			value._type = type;
			value._corners = corners.ToArray();
			_combinedObjs.Add(value);
			return value._id;
		}
	}
}
