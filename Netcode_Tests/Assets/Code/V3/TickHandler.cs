using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;

//#define UNITY_SERVER
namespace NT3 {
	public class TickHandler : Singelton<TickHandler> {

		int m_currentTick = 0;

		public static System.Action<uint> s_DoTick = null;

#if UNITY_SERVER
        class Client {//weil c# dum ist und mir eine kopie des structs zurück giebt anstadt eine reference auf das struct (Ligt an List bei array würde es funktionieren)
            public bool m_isConnected;
            public IPEndPoint m_eP;
            public InputHandler m_inputHandler = new InputHandler();//highend = maxTick;
            public RingBuffer<GameState> m_gameStates = new RingBuffer<GameState>();//lowend = confirmedTick;
        }

        List<Client> m_clients = new List<Client>();
#else

		RingBuffer<GameState> m_gameStates = new RingBuffer<GameState>();
		[SerializeField] InputHandler m_input_;
		public InputHandler m_input {
			get => m_input_;
			private set => m_input_ = value;
		}
#endif

		public void SimulateUptoTick(int maxTick) {

		}

		private void FixedUpdate() {
			//client
			//	highend < currentTick
			//		NetworkPause
			//	get next tick != default
			//	nextTick != currentTick
			//		get last server tick
			//		lerp
			//	apply nextTick to live data
			//	DoTick
			//	new InputElement
		}

		public bool AddGameState(GameState newGameState, int tick) {
			if (tick < m_currentTick)
				return false;

			m_gameStates[tick] = newGameState;
			return true;
		}
	}
}