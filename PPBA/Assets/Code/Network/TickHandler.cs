using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//#define UNITY_SERVER
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
		public static InputState s_interfaceInputState;

		public static int s_currentTick { get; private set; } = 0;
		[SerializeField] private int _inputBuffer;

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
				if(it._isConnected && it._inputStates.GetHighEnd() < min)//Game goes on if some clients disconnect
					min = it._inputStates.GetHighEnd();
			}

			if(min == int.MaxValue)
				yield break;

			for(; s_currentTick < min; s_currentTick++)
			{
				s_interfaceInputState = new InputState();
				for(int i = 0; i < GlobalVariables.s_clients.Count; i++) //combines inputs from all clients
				{
					foreach(var it in GlobalVariables.s_clients[i]._inputStates[s_currentTick]._objs)
					{
						it._client = GlobalVariables.s_clients[i]._id;
						s_interfaceInputState._objs.Add(it);
					}
					foreach(var it in GlobalVariables.s_clients[i]._inputStates[s_currentTick]._combinedObjs)
					{
						it._client = GlobalVariables.s_clients[i]._id;
						s_interfaceInputState._combinedObjs.Add(it);
					}
				}

#if UNITY_SERVER
				Time.timeScale = 8;
				while(Time.timeSinceLevelLoad < s_currentTick * Time.fixedDeltaTime)
					yield return null;
				Time.timeScale = 0;
#else
				s_interfaceGameState = GlobalVariables.s_clients[0]._gameStates[s_currentTick];
#endif

				s_DoInput?.Invoke(s_currentTick);
				s_EarlyCalc?.Invoke(s_currentTick);
				s_LateCalc?.Invoke(s_currentTick);
				s_AIEvaluate?.Invoke(s_currentTick);
				s_DoTick?.Invoke(s_currentTick);
			}

			s_interfaceGameState = new GameState();
			s_GatherValues?.Invoke(s_currentTick);

			foreach(var it in GlobalVariables.s_clients)
			{
				//TODO: split gamestate into only relevant data for client
				it._gameStates[s_currentTick] = s_interfaceGameState;
			}
#if UNITY_SERVER
			GameNetcode.s_instance.Send(s_currentTick);
#endif
			h_SimulationIsRunning = false;
		}

#if !UNITY_SERVER
		private void FixedUpdate()
		{
			client me = GlobalVariables.s_clients[0];

			if(me._gameStates.GetHighEnd() < s_currentTick ||
			  (me._gameStates.GetHighEnd() == s_currentTick && !me._gameStates[s_currentTick]._receivedMessages.AreAllBytesActive()))
			{
				Debug.Log("Network Pause");
				return;
			}

			GameState nextState = default;
			int nextStateTick = s_currentTick;
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

			if(nextStateTick != s_currentTick)
			{
				nextState = GameState.Lerp(me._gameStates[nextState._refTick], nextState, (s_currentTick - nextState._refTick) / (nextStateTick - nextState._refTick));
			}
			else
			{
				nextState.DismantleDelta(me._gameStates[nextState._refTick]);
			}

			s_interfaceGameState = nextState;
			s_interfaceInputState = me._inputStates[nextStateTick];

			s_DoInput?.Invoke(s_currentTick);
			s_EarlyCalc?.Invoke(s_currentTick);
			s_LateCalc?.Invoke(s_currentTick);
			s_AIEvaluate?.Invoke(s_currentTick);
			s_DoTick?.Invoke(s_currentTick);

			s_interfaceInputState = new InputState();

			s_GatherValues?.Invoke(s_currentTick + _inputBuffer);

			me._inputStates[s_currentTick + _inputBuffer] = s_interfaceInputState;

			s_currentTick++;
		}
#endif
	}
}