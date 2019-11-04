using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace PPBA
{
	public enum Behavior : byte {IDLE };

	[System.Flags]
	public enum Arguments : byte {
		NON = 0,
		ENABLED = 1,//2^0
		TRIGGERBEHAVIOUR = 2,//2^1
	};

	namespace GSC //Game State Component
	{
		public class gsc
		{
			public int _id;
		}

		public class type : gsc
		{
			public byte _type;
			public byte _team;
		}

		public class arg : gsc
		{
			public Arguments _arguments;
		}

		public class transform : gsc
		{
			public Vector3 _position;
			public float _angle;
		}

		public class ammo : gsc
		{
			public int _bullets;
			//public int _grenades;
		}

		public class resource : gsc
		{
			public int _resources;
		}
		
		public class health : gsc
		{
			public float _health;
			public float _morale;
		}

		public class behavior
		{
			public Behavior _behavior;
			public int _target;
		}

		public class path : gsc
		{
			public List<Vector3> _path;
		}

		public class map : gsc
		{
			public BitField2D _mask;
			List<float> _values;
		}
	}
	public class GameState
	{
		public int _refTick = -1;
		public byte _messageCount;
		public BitField2D _receivedMessages;
		public bool _isLerped;
		public bool _isDelta;
		private List<byte[]> _messageHolder = null;

		public List<GSC.type> _types = new List<GSC.type>();
		public List<GSC.arg> _args = new List<GSC.arg>();
		public List<GSC.transform> _transforms = new List<GSC.transform>();
		public List<GSC.ammo> _ammos = new List<GSC.ammo>();
		public List<GSC.resource> _resources = new List<GSC.resource>();
		public List<GSC.health> _healths = new List<GSC.health>();
		public List<GSC.behavior> _behaviors = new List<GSC.behavior>();
		public List<GSC.path> _paths = new List<GSC.path>();
		public List<GSC.map> _maps = new List<GSC.map>();
		public List<int> _denyedInputIDs = new List<int>();

		public List<byte[]> Encrypt(int maxPackageSize)
		{
			if(_messageHolder != null)
				return _messageHolder;

			throw new NotImplementedException();
		}

		public void Decrypt(byte[] msg, int offset)
		{
			throw new NotImplementedException();
		}

		public bool CreateDelta(RingBuffer<GameState> reference, int tick, int length)
		{
			throw new NotImplementedException();
		}

		public bool DismantleDelta(GameState reference, List<int> expactedInputs)
		{
			throw new NotImplementedException();
		}

		public bool Lerp(GameState start, GameState end, int t)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// packs the message into the next best package
		/// </summary>
		/// <param name="maxPackageSize">the maximum size of packages</param>
		/// <param name="packages">the list of already existing packages</param>
		/// <param name="additionalMessage">the message that should be inserted</param>
		void HandlePackageSize(int maxPackageSize, List<byte[]> packages, byte[] additionalMessage)
		{
			int index = -1;
			int remainder = int.MaxValue;

			for(int i = 0; i < packages.Count; i++)
			{
				int currentRefmainter = maxPackageSize - (packages[i].Length + additionalMessage.Length);
				if(currentRefmainter >= 0 && currentRefmainter < remainder)
				{
					remainder = currentRefmainter;
					index = i;
				}
			}

			if(index < 0)
			{
				packages.Add(additionalMessage);
			}
			else
			{
				byte[] tmp = new byte[packages[index].Length + additionalMessage.Length];
				Buffer.BlockCopy(packages[index], 0, tmp, 0, packages[index].Length);
				Buffer.BlockCopy(additionalMessage, 0, tmp, packages[index].Length, additionalMessage.Length);
				packages[index] = tmp;
			}
		}
	}
}
