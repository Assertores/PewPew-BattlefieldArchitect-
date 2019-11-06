using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public class Blueprint : MonoBehaviour
	{
		[SerializeField] public int _id = 0;
		[SerializeField] public int _team = 0;
		[SerializeField] public int _resources = 0;
		[SerializeField] public int _resourcesIncoming = 0;
		[SerializeField] public int _resourcesMax = 100;
		[SerializeField] public int _work = 0;
		[SerializeField] public int _workMax = 50;

		[SerializeField] public List<Pawn> _workers = new List<Pawn>();

		public float _workDoable
		{
			get => ((float)(_resources * _resourcesMax) / (float)_workMax) - (float)_work;
		}

		public float _resourcesNeeded
		{
			get => (float)(_resourcesMax - _resources - _resourcesIncoming);
		}

		// Start is called before the first frame update
		void Start()
		{

		}

		// Update is called once per frame
		void Update()
		{

		}

		public void WorkTick()
		{
			foreach(Pawn w in _workers)
			{
				_work += 1;
			}

			if(_workMax <= _work)
				WorkIsFinished();
		}

		public void DoWork(uint amount = 1)
		{
			_work += (int)amount;
			
			if(_workMax <= _work)
				WorkIsFinished();
		}

		private void WorkIsFinished()
		{
			if(JobCenter.s_blueprints[_team].Contains(this))
				JobCenter.s_blueprints[_team].Remove(this);

			//exchange blueprint for building
		}

		#region Initialisation
		private void OnEnable()
		{
			if(!JobCenter.s_blueprints[_team].Contains(this))
				JobCenter.s_blueprints[_team].Add(this);

			TickHandler.s_GatherValues += WriteToGameState;
		}

		private void OnDisable()
		{
			if(JobCenter.s_blueprints[_team].Contains(this))
				JobCenter.s_blueprints[_team].Remove(this);

			TickHandler.s_GatherValues -= WriteToGameState;
		}

		public void WriteToGameState(int tick)
		{
			TickHandler.s_interfaceGameState._transforms.Add(new GSC.transform { _id = _id, _position = transform.position, _angle = transform.eulerAngles.y });
			TickHandler.s_interfaceGameState._resources.Add(new GSC.resource { _id = _id, _resources = _resources });			
			TickHandler.s_interfaceGameState._works.Add(new GSC.work { _id = _id, _work = _work});
		}
		#endregion
	}
}