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
			public List<Vector3> _corners;
		}
	}

	public class InputState : MonoBehaviour
	{
		public List<ISC.obj> _objs = new List<ISC.obj>();
		public List<ISC.combinedObj> _combinedObjs = new List<ISC.combinedObj>();

		/// <summary>
		/// 
		/// </summary>
		/// <param name="type">type of the placed object</param>
		/// <param name="pos">position of the placed object</param>
		/// <param name="angle">rotation of the placed object around the y axis</param>
		/// <returns>input identifyer</returns>
		public int AddObj(ObjectType type, Vector3 pos, float angle)
		{
			return -1;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="type">type of the placed object</param>
		/// <param name="corners">list of corner positions</param>
		/// <returns>input identifyer</returns>
		public int AddCombinedObj(ObjectType type, List<Vector3> corners)
		{
			return -1;
		}
	}
}