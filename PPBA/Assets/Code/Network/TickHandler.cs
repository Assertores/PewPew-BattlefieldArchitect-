using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//#define UNITY_SERVER
namespace PPBA
{
	public class TickHandler : Singleton<TickHandler>
	{
		public static Action<int> s_SetUp;
		public static Action<int> s_DoInput;
		public static Action<int> s_EarlyCalc;
		public static Action<int> s_LateCalc;
		public static Action<int> s_AIEvaluate;
		public static Action<int> s_DoTick;
		public static Action<int> s_GatherValues;

		public static GameState s_interfaceGameState;
		public static InputState s_interfaceInputState;

		public static int s_currentTick { get; private set; } = 0;
		public static float s_currentTickTime = 0.0f; //referenced to Time.time
		[SerializeField] private int _inputBuffer = 6;

		private void Start()
		{
#if UNITY_SERVER
			Time.timeScale = 0;
#else
			for(int i = 0; i < _inputBuffer; i++)
			{
				GlobalVariables.s_instance._clients[0]._inputStates[i] = new InputState();
				//print(GlobalVariables.s_instance._clients[0]._inputStates.GetHighEnd());
			}
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

			Debug.Log("[Server] Simulating");

			int min = int.MaxValue;
			foreach(var it in GlobalVariables.s_instance._clients)
			{
				if(it._isConnected && it._inputStates.GetHighEnd() < min)//Game goes on if some clients disconnect
					min = it._inputStates.GetHighEnd();
			}

			Debug.Log("[Server] min value: " + min);

			if(min == int.MaxValue)
			{
				h_SimulationIsRunning = false;
				yield break;
			}
			if(s_currentTick >= min)
			{
				h_SimulationIsRunning = false;
				yield break;
			}

			for(s_currentTick++; s_currentTick <= min; s_currentTick++)
			{
				Debug.Log("[Server] Simulating tick: " + s_currentTick);
				s_interfaceInputState = new InputState();
				for(int i = 0; i < GlobalVariables.s_instance._clients.Count; i++) //combines inputs from all clients
				{
					foreach(var it in GlobalVariables.s_instance._clients[i]._inputStates[s_currentTick]._objs)
					{
						it._client = GlobalVariables.s_instance._clients[i]._id;
						s_interfaceInputState._objs.Add(it);
					}
					foreach(var it in GlobalVariables.s_instance._clients[i]._inputStates[s_currentTick]._combinedObjs)
					{
						it._client = GlobalVariables.s_instance._clients[i]._id;
						s_interfaceInputState._combinedObjs.Add(it);
					}
				}
				Debug.Log("[Server] combined inputs");

#if UNITY_SERVER
				Time.timeScale = 8;
				while(Time.timeSinceLevelLoad < s_currentTick * Time.fixedDeltaTime)
					yield return null;
				Time.timeScale = 0;
#else
				s_interfaceGameState = GlobalVariables.s_instance._clients[0]._gameStates[s_currentTick];
#endif

				s_SetUp?.Invoke(s_currentTick);
				s_DoInput?.Invoke(s_currentTick);
				s_EarlyCalc?.Invoke(s_currentTick);
				s_LateCalc?.Invoke(s_currentTick);
				s_AIEvaluate?.Invoke(s_currentTick);
				s_DoTick?.Invoke(s_currentTick);
			}
			s_currentTick--;

			Debug.Log("[Server] Finished simulating");

			s_interfaceGameState = new GameState();
			s_interfaceInputState = new InputState();

			s_GatherValues?.Invoke(s_currentTick);

			Debug.Log("[Server] Seperating Gamestate");
			foreach(var it in GlobalVariables.s_instance._clients)
			{
				Debug.Log("[Server] for client: " + it._id);
				GameState element = new GameState(s_interfaceGameState);

				element._denyedInputIDs = element._denyedInputIDs.FindAll(x => x._client == it._id);

				it._gameStates[s_currentTick] = element;
			}
#if UNITY_SERVER
			Debug.Log("[Server] Sending gameState for tick: " + s_currentTick);
			GameNetcode.s_instance.Send(s_currentTick);
#endif
			h_SimulationIsRunning = false;
		}

#if !UNITY_SERVER
		private void FixedUpdate()
		{
			client me = GlobalVariables.s_instance._clients[0];

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

			foreach(var it in nextState._newIDRanges)
			{
				bool exists = false;
				for(int i = nextState._refTick; i < nextStateTick; i++)
				{
					foreach(var jt in GlobalVariables.s_instance._clients[0]._gameStates[i]._newIDRanges)
					{
						if(jt == default)
							continue;
						if(jt._id == it._id)
						{
							exists = true;
							break;
						}
					}
					if(exists)
						break;
				}

				if(!exists)
					ObjectPool.s_objectPools[GlobalVariables.s_instance._prefabs[(int)it._type]]?.Resize(it._range, it._id);
			}

			s_interfaceGameState = nextState;
			s_interfaceInputState = me._inputStates[nextStateTick];

			s_currentTickTime = Time.time;

			s_DoInput?.Invoke(s_currentTick);
			s_EarlyCalc?.Invoke(s_currentTick);
			s_LateCalc?.Invoke(s_currentTick);
			s_AIEvaluate?.Invoke(s_currentTick);
			s_DoTick?.Invoke(s_currentTick);

			s_interfaceInputState = new InputState();
			s_interfaceGameState = new GameState();

			s_GatherValues?.Invoke(s_currentTick + _inputBuffer);

			me._inputStates[s_currentTick + _inputBuffer] = s_interfaceInputState;

			s_currentTick++;
		}
#endif
	}
}
