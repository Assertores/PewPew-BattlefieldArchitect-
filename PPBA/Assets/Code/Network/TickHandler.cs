using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace PPBA
{
	public class TickHandler : Singleton<TickHandler>
	{
		public static Action<int> s_DoInput;
		public static Action<int> s_EarlyCalc;
		public static Action<int> s_LateCalc;
		public static Action<int> s_AIEvaluate;
		public static Action<int> s_DoTick;
		public static Action<int> s_GatherValues;

		public static GameState s_interfaceGameState;

		private int _currentTick;
		private int _inputTick;

		private void Start()
		{
#if UNITY_SERVER
			Time.timeScale = 0;
#endif
		}

		public void Simulate()
		{
			if(!h_SimulationIsRunning)
				StartCoroutine(IESimulate());
		}

		bool h_SimulationIsRunning = false;
		IEnumerator IESimulate()
		{
			h_SimulationIsRunning = true;
			int min = int.MaxValue;
			foreach(var it in GlobalVariables.s_clients)
			{
				if(it._isConnected /*&& it._inputBuffer.GetHighEnd() < min*/)//Game goes on if some clients disconnect
					;//min = it._inputBuffer.GetHighEnd();
			}

			if(min == int.MaxValue)
				yield break;

			for(; _currentTick < min; _currentTick++)
			{
				for(int i = 0; i < GlobalVariables.s_clients.Count; i++)
				{
					//combine Inputs
				}

#if UNITY_SERVER
				Time.timeScale = 8;
				while(Time.timeSinceLevelLoad < _currentTick * Time.fixedDeltaTime)
					yield return null;
				Time.timeScale = 0;
#endif

				s_DoInput?.Invoke(_currentTick);
				s_EarlyCalc?.Invoke(_currentTick);
				s_LateCalc?.Invoke(_currentTick);
				s_AIEvaluate?.Invoke(_currentTick);
				s_DoTick?.Invoke(_currentTick);
			}

			s_interfaceGameState = new GameState();
			s_GatherValues?.Invoke(_currentTick);

			foreach(var it in GlobalVariables.s_clients)
			{
				it._gameStates[_currentTick] = s_interfaceGameState;
			}
#if UNITY_SERVER
			//TODO: Netcode stard sending Gamestates to Clients
#endif
			h_SimulationIsRunning = false;
		}

#if !UNITY_SERVER
		private void FixedUpdate()
		{
			client me = GlobalVariables.s_clients[0];

			if(me._gameStates.GetHighEnd() < _currentTick || !me._gameStates[_currentTick]._receivedMessages.AreAllBytesActive())
			{
				Debug.Log("Network Pause");
				return;
			}

			GameState nextState = default;
			int nextStateTick = _currentTick;
			for(; nextState == default; nextStateTick++)
			{
				nextState = me._gameStates[nextStateTick];
			}
			nextStateTick--;//nextStateTick++ will be executed once to often

			if(nextState._refTick < me._gameStates.GetLowEnd() || me._gameStates[nextState._refTick] == default)
			{
				Debug.LogError("Reference Tick not Found");
				return;//no idea how to fix this
			}

			if(nextStateTick != _currentTick)
			{
				nextState = GameState.Lerp(me._gameStates[nextState._refTick], nextState, (_currentTick - nextState._refTick) / (nextStateTick - nextState._refTick));
			}
			else
			{
				nextState.DismantleDelta(me._gameStates[nextState._refTick], null/*me._inputBuffer[nextStateTick].GetInputIDs()*/);
			}

			s_interfaceGameState = nextState;

			s_DoInput?.Invoke(_currentTick);
			s_EarlyCalc?.Invoke(_currentTick);
			s_LateCalc?.Invoke(_currentTick);
			s_AIEvaluate?.Invoke(_currentTick);
			s_DoTick?.Invoke(_currentTick);
			//rest InterfaceInputState
			s_GatherValues?.Invoke(_currentTick);
			//me._inputBuffer.AddNewElement(InterfaceInputState);
		}
#endif
	}
}