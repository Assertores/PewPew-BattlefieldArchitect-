using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Net;
using System.Net.Sockets;

namespace T2 {

    /// <summary>
    /// ===== ===== Package Layout ===== =====
    /// every packages first byte descripes the package type (MessageType)
    /// Client -> Server:
    /// NON:        byte Type, int ID, {uint tick, int size, InputType[] inputs}[] tickInputs
    /// CONNECT:    byte Type
    /// DISCONNECT: byte Type, int ID
    /// RECONNECT:  byte Type, int ID
    /// Server -> Client:
    /// NON:        byte Type, int Tick, int RefTick, Gamestate[] states
    /// NEWID:      byte Type, int ID
    /// ===== ===== GameState buffer ===== =====
    /// server buffers all gamestates of all players upto the last confirmed tick, the server resived from that client
    /// client buffers all gamestates                upto the reference tick of current tick
    /// ===== ===== ===== =====
    /// </summary>

    enum MessageType : byte { NON, CONNECT, DISCONNECT, RECONNECT, NEWID }

//#define UNITY_SERVER
    public class LockstepTest2 : MonoBehaviour {

        public static Action<uint> DoTick;

        uint m_currentTick = 0;
        UdpClient socket;

        public int m_serverPort = 11000;

#if UNITY_SERVER
        //array von allen inputs queues mit id und lastIPEndPoint und maxtickimput
        class Client {//weil c# dum ist und mir eine kopie des structs zurück giebt anstadt eine reference auf das struct (Ligt an List bei array würde es funktionieren)
            public bool isConnected;
            public IPEndPoint eP;
            public uint maxTick;
            public uint confirmedTick;
            public FaceInput inputHandler;
            public List<Gamestate> gameStates;//TODO: change to linked list
        }

        List<Client> clients = new List<Client>();
        public static Gamestate currentGameState;

        int nextID = 0;
#else
        IPEndPoint ep;
        public string m_iP = "127.0.0.1";

        List<Gamestate> upComingStates = new List<Gamestate>();//deltas
        List<Gamestate> pastStates = new List<Gamestate>();//real states
        [SerializeField] FaceInput m_input;
#endif
#if UNITY_SERVER
        void Start() {
            socket = new UdpClient(11000);
            Debug.Log("[Server] server is ready and lisents");
            socket.DontFragment = true;
        }
        private void OnDestroy() {
            socket.Close();
        }

        void Update() {
            if (!Listen())
                return;

            uint min = uint.MaxValue;
            foreach (var it in clients) {
                if (min > it.maxTick)
                    min = it.maxTick;
            }
            if (min <= m_currentTick)
                return;

            for (uint i = m_currentTick + 1; i <= min; i++) {
                Debug.Log("[Server] simulating tick: " + i);
                DoTick?.Invoke(i);
            }
            m_currentTick = min;

            Gamestate currentState = FaceGamestateHandler.s_singelton.CreateGameState(m_currentTick);
            foreach (var it in clients) {
                it.gameStates.Add(currentState);
            }

            Debug.Log("[Server] dequeuing inputs of all clients upto tick: " + m_currentTick);
            foreach (var it in clients) {
                it.inputHandler.DequeueUptoTick(m_currentTick);
            }

            Send();
        }

        bool Listen() {
            if (socket.Available <= 0)
                return false;

            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 11000);
            byte[] data = socket.Receive(ref remoteEP);

            Debug.Log("[Server] reseaved package");

            MessageType messageType = (MessageType)data[0];
            switch (messageType) {
            case MessageType.NON:
                HandleNON(data, remoteEP);
                break;
            case MessageType.CONNECT:
                HandleConnect(data, remoteEP);
                break;
            case MessageType.DISCONNECT:
                HandleDisconnect(data, remoteEP);
                break;
            case MessageType.RECONNECT:
                HandleReconnect(data, remoteEP);
                break;
            default:
                Debug.Log("[Server] package type was not handled" + messageType);
                break;
            }

            return true;
        }
#else
        void Start() {
            socket = new UdpClient();
            ep = new IPEndPoint(IPAddress.Parse(m_iP), m_serverPort); // endpoint where server is listening
            socket.Connect(ep);
            socket.DontFragment = true;

            byte[] msg = new byte[1];
            msg[0] = (byte)MessageType.CONNECT;
            socket.Send(msg, msg.Length);

            m_input.CreateNewElement(m_currentTick);
        }
        private void OnDestroy() {
            byte[] msg = new byte[1 + sizeof(int)];
            msg[0] = (byte)MessageType.DISCONNECT;
            Buffer.BlockCopy(BitConverter.GetBytes(m_input.m_iD), 0, msg, 1, sizeof(int));
            socket.Send(msg, msg.Length);

            socket.Close();
        }

        private void Update() {
            Listen();
        }

        /// <summary>
        /// Send Player Inputs
        /// if next tick is available {
        ///     calculates GameState from reference GameState and delta data
        /// } else {
        ///     check if future tick is available (interpolate)
        ///     if not
        ///         Network pause
        /// }
        /// dequeues all gamestate previus to reference GameState
        /// dequeue all upcomming gamestate previus to current gametick
        /// set Live data to GameState
        /// Make Client GameTick
        /// tick increment
        /// </summary>
        private void FixedUpdate() {
            Send();

            if (!upComingStates.Exists(x => x.tick > m_currentTick)) {
                upComingStates.Clear();
                print("Network Pause");
                return;
            }

            uint min = uint.MaxValue;
            foreach (var it in upComingStates) {
                if (min > it.tick)
                    min = it.tick;
            }

            Gamestate currentGamestate = upComingStates.Find(x => x.tick == min);

            if (currentGamestate.tick > m_currentTick) {
                uint max = 0;
                foreach (var it in pastStates) {
                    if (max < it.tick)
                        max = it.tick;
                }
                currentGamestate.Lerp(pastStates.Find(x => x.tick == max), m_currentTick);
            } else {
                upComingStates.Remove(currentGamestate);
                currentGamestate.CreateFullTick(pastStates.Find(x => x.tick == currentGamestate.refTick));
                pastStates.Add(currentGamestate);
            }
            pastStates.RemoveAll(x => x.tick < currentGamestate.refTick);
            
            FaceGamestateHandler.s_singelton.ApplyGameState(currentGamestate);
            DoTick?.Invoke(m_currentTick);

            m_currentTick++;

            //TODO: was pasiert mit input während einem Network Pause
            m_input.CreateNewElement(m_currentTick);
        }

        void Listen() {
            if (socket.Available <= 0)
                return;

            byte[] data = socket.Receive(ref ep);

            MessageType messageType = (MessageType)data[0];

            switch (messageType) {
            case MessageType.NON:
                HandleNON(data);
                break;
            case MessageType.NEWID:
                HandleNewID(data);
                break;
            default:
                break;
            }
        }
#endif
#if UNITY_SERVER
        /// NON:        byte Type, int ID, {uint tick, int size, InputType[] inputs}[] tickInputs
        void HandleNON(byte[] data, IPEndPoint ep) {
            int id = BitConverter.ToInt32(data, 1);
            Client client = clients.Find(x => x.inputHandler.m_iD == id);

            if (client == null)
                return;

            client.eP = ep;

            uint min = uint.MaxValue;
            int offset = sizeof(int) + sizeof(MessageType);
            while (offset < data.Length) {
                d_Input IMelement = new d_Input();
                offset += IMelement.Decrypt(data, offset);
                if (IMelement.tick > client.maxTick) {
                    client.inputHandler.AddNewElement(IMelement);
                    client.maxTick = IMelement.tick;
                }
                if (min > IMelement.tick)
                    min = IMelement.tick;
            }

            

            if (client.confirmedTick < min - 1 && min - 1 <= m_currentTick) {
                client.confirmedTick = min - 1;

                if (client.confirmedTick < m_currentTick)
                    client.gameStates.RemoveAll(x => x.tick < client.confirmedTick);
            }

            Debug.Log("[Server] client " + client.inputHandler.m_iD + " has send new inputs of the tickrange " + client.confirmedTick + " - " + client.maxTick);
        }
#else
        /// NON:        byte Type, int ID, {uint tick, int size, InputType[] inputs}[] tickInputs
        void Send() {
            byte[] msg = m_input.Encrypt();

            socket.Send(msg, msg.Length);
        }
#endif
#if UNITY_SERVER
        /// NON:        byte Type, int Tick, int RefTick, Gamestate[] states
        void Send() {
            foreach (var it in clients) {
                if (!it.isConnected)
                    continue;

                Gamestate currentStateCopy = it.gameStates.Find(x => x.tick == m_currentTick);
                Gamestate confirmedStateCopy = it.gameStates.Find(x => x.tick == it.confirmedTick);
                currentStateCopy.CreateDelta(confirmedStateCopy);

                byte[] enc = currentStateCopy.Encrypt();
                byte[] msg = new byte[enc.Length + 1];
                msg[0] = (byte)MessageType.NON;
                Buffer.BlockCopy(enc, 0, msg, 1, enc.Length);

                Debug.Log("[Server] sending gamestate delta of tick " + currentStateCopy.tick + " with reference to tick " + currentStateCopy.refTick);

                socket.Send(msg, msg.Length, it.eP);
            }
        }
#else
        //dequeue confirmed inputs
        //check if package gamestate is newer then current gamestate
        //add gamestate to upcomingStates
        /// NON:        byte Type, int Tick, int RefTick, Gamestate[] states
        void HandleNON(byte[] data) {
            Gamestate element = new Gamestate();
            element.Decrypt(data, 1);

            if (element.tick <= m_currentTick)
                return;

            upComingStates.Add(element);
            m_input.DequeueUptoTick(element.tick);
        }
#endif
#if UNITY_SERVER
        /// CONNECT:    byte Type
        /// NEWID:      byte Type, int ID
        void HandleConnect(byte[] data, IPEndPoint ep) {
            Client element = new Client();

            element.isConnected = true;
            element.maxTick = 0;
            element.confirmedTick = 0;
            element.eP = ep;
            GameObject tmp = new GameObject();
            element.inputHandler = tmp.AddComponent<FaceInput>();
            element.inputHandler.m_iD = nextID;
            element.gameStates = new List<Gamestate>();

            clients.Add(element);

            Debug.Log("[Server] new client connected. id: " + element.inputHandler.m_iD);

            byte[] msg = new byte[1 + sizeof(int)];
            msg[0] = (byte)MessageType.NEWID;
            Buffer.BlockCopy(BitConverter.GetBytes(nextID), 0, msg, 1, sizeof(int));
            socket.Send(msg, msg.Length, ep);
            nextID++;
        }

        /// DISCONNECT: byte Type, int ID
        void HandleDisconnect(byte[] data, IPEndPoint ep) {
            int RemoteID = BitConverter.ToInt32(data, 1);
            Client element = clients.Find(x => x.inputHandler.m_iD == RemoteID);
            if (element == null)
                return;

            element.isConnected = false;
            element.eP = ep;

            Debug.Log("[Server] client " + element.inputHandler.m_iD + " disconnected");
        }

        /// RECONNECT:  byte Type, int ID
        void HandleReconnect(byte[] data, IPEndPoint ep) {
            int RemoteID = BitConverter.ToInt32(data, 1);
            Client element = clients.Find(x => x.inputHandler.m_iD == RemoteID);
            if (element != null) {
                element.isConnected = true;
                element.eP = ep;

                Debug.Log("[Server] client " + element.inputHandler.m_iD + " Reconnected");
            } else {
                HandleConnect(data, ep);
            }
        }
#else
        void HandleNewID(byte[] data) {
            m_input.m_iD = BitConverter.ToInt32(data, 1);
        }
#endif
    }
}