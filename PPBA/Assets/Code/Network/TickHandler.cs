using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Profiling;

//#define UNITY_SERVER
namespace PPBA
{
	public class TickHandler : Singleton<TickHandler>
	{
		public static bool s_NetworkPause { get; private set; } = false;

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
		[SerializeField] private TMPro.TextMeshProUGUI _debugText;

#if !UNITY_SERVER
		private void Start()
		{
			for(int i = 0; i < _inputBuffer; i++)
			{
				GlobalVariables.s_instance._clients[0]._inputStates[i] = new InputState();
			}
		}

		int h_catchUpTick = 0;
		private void Update()
		{
			if(h_catchUpTick > s_currentTick)
				TickIt();
		}

		private void FixedUpdate()
		{
			TickIt();

			if(s_currentTick > 2)
				h_catchUpTick++;
		}
#endif

		public int Simulate()
		{
			int minClientID = -1;
			int min = int.MaxValue;
			foreach(var it in GlobalVariables.s_instance._clients)
			{
				if(it._isConnected && it._inputStates.GetHighEnd() < min)//Game goes on if some clients disconnect
				{
					minClientID = it._id;
					min = it._inputStates.GetHighEnd();
				}
			}
#if DB_NC
			Debug.Log("simulating up to: " + min + " with current tick beeing: " + s_currentTick);
#endif

			if(min == int.MaxValue)
			{
				return -1;
			}
			if(s_currentTick >= min)
			{
				return -2;
			}

			//Profiler.BeginSample("[Server] Simulating all");
			for(s_currentTick++; s_currentTick <= min; s_currentTick++)
			{
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
					foreach(var it in GlobalVariables.s_instance._clients[i]._inputStates[s_currentTick]._produceUnits)
					{
						it._client = GlobalVariables.s_instance._clients[i]._id;
						s_interfaceInputState._produceUnits.Add(it);
					}
				}

#if !UNITY_SERVER
				s_interfaceGameState = GlobalVariables.s_instance._clients[0]._gameStates[s_currentTick];
#endif
				//Profiler.BeginSample("[Server] SetUp");
				s_SetUp?.Invoke(s_currentTick);
				//Profiler.EndSample();
				//Profiler.BeginSample("[Server] Input");
				s_DoInput?.Invoke(s_currentTick);
				//Profiler.EndSample();
				//Profiler.BeginSample("[Server] Calc1");
				s_EarlyCalc?.Invoke(s_currentTick);
				//Profiler.EndSample();
				//Profiler.BeginSample("[Server] Calc2");
				s_LateCalc?.Invoke(s_currentTick);
				//Profiler.EndSample();
				//Profiler.BeginSample("[Server] Evaluation");
				s_AIEvaluate?.Invoke(s_currentTick);
				//Profiler.EndSample();
				//Profiler.BeginSample("[Server] Tick");
				s_DoTick?.Invoke(s_currentTick);
				//Profiler.EndSample();

#if DB_NC
				if(null != _debugText)
					_debugText.text = "Tick: " + s_currentTick + " -> " + minClientID;
#endif
			}
			s_currentTick--;
			//Profiler.EndSample();
			//Profiler.BeginSample("[Server] Gather Values");
			s_interfaceGameState = new GameState();
			s_interfaceInputState = new InputState();

			s_GatherValues?.Invoke(s_currentTick);
			//Profiler.EndSample();

#if DB_NC
			if(s_currentTick % 20 == 0)
				Debug.Log("Tick: " + s_currentTick + "\n" + s_interfaceGameState.ToString());
#endif

			//Profiler.BeginSample("[Server] Seperating GS");
			foreach(var it in GlobalVariables.s_instance._clients)
			{
				GameState element = new GameState(s_interfaceGameState);

				element._denyedInputIDs = element._denyedInputIDs.FindAll(x => x._client == it._id);
				element._scheduledPawns = element._scheduledPawns.FindAll(x => x._id == it._id);

				it._gameStates[s_currentTick] = element;

				it._inputStates.FreeUpTo(s_currentTick);
			}
			//Profiler.EndSample();

			return s_currentTick;
		}

		UIPopUpWindowRefHolder h_popUp;
		private bool TickIt()
		{
			client me = GlobalVariables.s_instance._clients[0];

			if(me._gameStates.GetHighEnd() < s_currentTick ||
			  (me._gameStates.GetHighEnd() == s_currentTick && !me._gameStates[s_currentTick]._receivedMessages.AreAllBytesActive()))
			{
				s_NetworkPause = true;
#if DB_NC
				Debug.Log("Network Pause");
#endif

				if(null == h_popUp)
					h_popUp = UIPopUpWindowHandler.s_instance.CreateWindow("Network Pause");

				return false;
			}

			if(s_NetworkPause && me._gameStates.GetHighEnd() - s_currentTick <= _inputBuffer / 2)//not quite shure. feals right. might get stuck in an deadlock otherwise.
			{
#if DB_NC
				Debug.Log("waiting for buffer refilling");
#endif
				return false;
			}

#if DB_NC
			Debug.Log("Start serch at: " + s_currentTick);
#endif
			GameState nextState = default;
			int nextStateTick = s_currentTick;
			if(s_currentTick == 0)
			{
				nextState = new GameState();
			}
			else
			{
				for(; nextStateTick <= me._gameStates.GetHighEnd() && (nextState == default || nextState._isNULLGameState); nextStateTick++)
				{
					nextState = me._gameStates[nextStateTick];
#if DB_NC
					if(nextState == default)
					{
						Debug.Log("Serch: " + nextStateTick + ", no State");
					}
					else
					{
						Debug.Log("Serch: " + nextStateTick + ", " + nextState._isNULLGameState);
					}
#endif
				}
				if(nextState == default)
					return false;
				nextStateTick--;//nextStateTick++ will be executed once to often
			}
			

			if(null != h_popUp)
			{
				h_popUp.CloseWindow();
				h_popUp = null;
			}
			s_NetworkPause = false;

			if(nextState._refTick < me._gameStates.GetLowEnd() || me._gameStates[nextState._refTick] == default)
			{
#if DB_NC
				Debug.Log(nextStateTick + " | ref: " + nextState._refTick);
				Debug.LogError("Reference Tick not Found");
#endif
				return false;//no idea how to fix this
			}


			if(nextStateTick != s_currentTick)
			{
				nextState = GameState.Lerp(me._gameStates[nextState._refTick], nextState, (s_currentTick - nextState._refTick) / (nextStateTick - nextState._refTick));
				me._gameStates[s_currentTick] = nextState;
			}
			else if(nextState._refTick != 0)
			{

				nextState.DismantleDelta(me._gameStates[nextState._refTick]);
			}

			me._gameStates.FreeUpTo(nextState._refTick - 1);

#if DB_OP
			Debug.Log("===== ===== TICK: " + s_currentTick + " ===== =====");
#endif
			if(!nextState._isNULLGameState && nextState._newIDRanges != null)
			{
				foreach(var it in nextState._newIDRanges)
				{
#if DB_OP
				Debug.Log("testing op: " + it._type);
#endif
					bool exists = false;
					for(int i = nextState._refTick; i < s_currentTick; i++)
					{
#if DB_OP
					Debug.Log("for tick " + i);
#endif
						if(default == me._gameStates[i])
							continue;

#if DB_OP
					Debug.Log("tick " + i + " exists");
#endif

						if(me._gameStates[i]._newIDRanges.Exists(x => x._id == it._id))
						{
							exists = true;
							break;
						}
					}

#if DB_OP
				Debug.Log("ids " + it._id + " in op " + it._type + " do exist? " + exists);
#endif

					if(!exists)
					{
						ObjectPool.s_objectPools[GlobalVariables.s_instance._prefabs[(int)it._type]]?.Resize(it._range, it._id);
					}
				}
			}

			s_interfaceGameState = nextState;
			s_interfaceInputState = me._inputStates[nextStateTick];

			s_currentTickTime = Time.time;

#if DB_GS
			if(s_currentTick % 20 == 0)
				Debug.Log("Tick: " + s_currentTick + "\n" + s_interfaceGameState.ToString());
#endif

			//Profiler.BeginSample("[Client] SetUp");
			s_SetUp?.Invoke(s_currentTick);
			//Profiler.EndSample();
			//Profiler.BeginSample("[Client] Input");
			s_DoInput?.Invoke(s_currentTick);
			//Profiler.EndSample();
			//Profiler.BeginSample("[Client] Calc1");
			s_EarlyCalc?.Invoke(s_currentTick);
			//Profiler.EndSample();
			//Profiler.BeginSample("[Client] Calc2");
			s_LateCalc?.Invoke(s_currentTick);
			//Profiler.EndSample();
			//Profiler.BeginSample("[Client] Evaluation");
			s_AIEvaluate?.Invoke(s_currentTick);
			//Profiler.EndSample();
			//Profiler.BeginSample("[Client] Tick");
			s_DoTick?.Invoke(s_currentTick);
			//Profiler.EndSample();

			s_interfaceInputState = new InputState();
			s_interfaceGameState = new GameState();

			s_GatherValues?.Invoke(s_currentTick + _inputBuffer);

			me._inputStates[s_currentTick + _inputBuffer] = s_interfaceInputState;

#if DB_NC
			if(null != _debugText)
				_debugText.text = "Tick: " + s_currentTick + " -> " + me._gameStates.GetHighEnd() + " (" + me._inputStates.GetLowEnd() + ", " + me._inputStates.GetHighEnd() + ")";
#endif

			s_currentTick++;

			return true;
		}
		public void DoReset()
		{
			s_currentTick = 0;
			s_currentTickTime = 0;
		}
	}
}
