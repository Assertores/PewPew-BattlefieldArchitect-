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

    #region structs

    enum MessageType : byte { NON, CONNECT, DISCONNECT, RECONNECT, NEWID }

    public enum InputType : byte { FORWARD, BACKWARD, LEFT, RIGHT, UP, DOWN }

    public struct Input {
        public uint tick;
        public InputType[] inputs;

        public byte[] Encrypt() {
            byte[] value = new byte[2 * sizeof(uint) + inputs.Length * sizeof(InputType)];
            Buffer.BlockCopy(BitConverter.GetBytes(tick), 0, value, 0, sizeof(uint));
            Buffer.BlockCopy(BitConverter.GetBytes(inputs.Length), 0, value, sizeof(uint), sizeof(int));
            Buffer.BlockCopy(inputs, 0, value, sizeof(uint) + sizeof(int), inputs.Length * sizeof(InputType));
            return value;
        }

        public int Decrypt(byte[] msg, int offset) {
            offset += sizeof(uint) + sizeof(int);
            tick = BitConverter.ToUInt32(msg, offset);
            int size = BitConverter.ToInt32(msg, offset + sizeof(uint));

            inputs = new InputType[size];

            Buffer.BlockCopy(msg, offset, inputs, 0, size * sizeof(InputType));
            return offset + size * sizeof(InputType);
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct GameStateItem {
        [FieldOffset(0)]
        public uint iD;
        [FieldOffset(sizeof(uint))]
        public int health;
        [FieldOffset(sizeof(uint) + sizeof(int))]
        public Vector3 pos;

        [FieldOffset(0)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = sizeof(uint) + sizeof(int) + 3 * sizeof(float))]
        public byte[] data;
    }

    //should be outsourced to a class
    public struct Gamestate {
        public uint tick;
        public bool isDelta;
        public bool isLerped;
        public uint refTick;
        public GameStateItem[] states;

        public byte[] Encrypt() {
            int sizeofGameState = sizeof(uint) + 3 * sizeof(float);
            byte[] value = new byte[2 * sizeof(uint) + states.Length * sizeofGameState];

            Buffer.BlockCopy(BitConverter.GetBytes(tick), 0, value, 0, sizeof(uint));
            Buffer.BlockCopy(BitConverter.GetBytes(refTick), 0, value, sizeof(uint), sizeof(uint));

            for (int i = 0; i < states.Length; i++) {
                Buffer.BlockCopy(states[i].data, 0, value, 2 * sizeof(uint) + i * sizeofGameState, sizeofGameState);
            }

            return value;
        }

        public void Decrypt(byte[] msg, int offset) {
            int sizeofGameState = sizeof(uint) + 3 * sizeof(float);
            tick = BitConverter.ToUInt32(msg, offset);
            offset += sizeof(uint);
            isDelta = true;
            isLerped = false;
            refTick = BitConverter.ToUInt32(msg, offset);
            offset += sizeof(uint);

            states = new GameStateItem[(msg.Length - offset) / sizeofGameState];
            for (int i = 0; i < states.Length; i++) {
                Buffer.BlockCopy(msg, offset + i * sizeofGameState, states[i].data, 0, sizeofGameState);
            }
        }

        public bool CreateFullTick(Gamestate reference) {
            if (reference.tick != refTick)
                return false;
            if (reference.isDelta)
                return false;

            List<GameStateItem> newState = new List<GameStateItem>(states);
            foreach (var it in reference.states) {
                if (!newState.Exists(x => x.iD == it.iD))
                    newState.Add(it);
            }

            states = newState.ToArray();
            isDelta = false;
            return true;
        }

        public bool CreateDelta(Gamestate reference) {
            if (reference.tick >= tick)
                return false;
            if (reference.isDelta)
                return false;

            List<GameStateItem> delta = new List<GameStateItem>();
            foreach (var it in states) {
                int index = -1;
                for (int i = 0; i < reference.states.Length; i++) {
                    if (reference.states[i].iD == it.iD) {
                        index = i;
                        break;
                    }
                }
                if (index == -1 ||
                    it.health != reference.states[index].health ||
                    it.pos != reference.states[index].pos) {
                    delta.Add(it);
                }
            }

            states = delta.ToArray();
            isDelta = true;
            refTick = reference.tick;
            return true;
        }

        public bool Lerp(Gamestate start, uint t) {
            if (start.tick >= tick)
                return false;
            if (start.tick >= t)
                return false;
            if (tick <= t)
                return false;
            if (start.isDelta)
                return false;

            float lv = (float)((double)t / (tick - start.tick));

            tick = t;
            List<GameStateItem> newState = new List<GameStateItem>(start.states);
            foreach (var it in states) {
                if (!newState.Exists(x => x.iD == it.iD) && lv > 0.5f) {
                    newState.Add(it);
                    continue;
                }
                int index = newState.FindIndex(x => x.iD == it.iD);
                GameStateItem element = newState[index];
                element.pos = Vector3.Lerp(element.pos, it.pos, lv);
                element.health = (int)Mathf.Lerp(element.health, it.health, lv);

                newState[index] = element;
            }

            states = newState.ToArray();

            isDelta = false;
            isLerped = true;
            refTick = start.refTick;
            return true;
        }
    }

    #endregion

    //#define UNITY_SERVER
    public class LockstepTest2 : MonoBehaviour {

        public static Action<uint> DoTick;

        uint m_currentTick = 0;
        UdpClient socket;

        public int m_serverPort = 11000;

#if UNITY_SERVER
        //array von allen inputs queues mit id und lastIPEndPoint und maxtickimput
        class Client {//weil c# dum ist und mir eine kopie des structs zurück giebt anstadt eine reference auf das struct (Ligt an List bei array würde es funktionieren)
            public int iD;
            public bool isConnected;
            public IPEndPoint eP;
            public uint maxTick;
            public uint confirmedTick;
            public List<Input> inputs;//TODO: change to linked list
            public List<Gamestate> gameStates;//TODO: change to linked list
        }

        List<Client> clients = new List<Client>();
        public static Gamestate currentGameState;

        int nextID = 0;
#else
        IPEndPoint ep;
        public string m_iP = "127.0.0.1";

        public int m_iD = -1;

        List<Gamestate> upComingStates = new List<Gamestate>();//deltas
        List<Gamestate> pastStates = new List<Gamestate>();//real states
        List<Input> unconfirmedInputs = new List<Input>();
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

            //TODO: simulate
            //TODO: set current tick on last simulated tick

            foreach(var it in clients) {
                it.inputs.RemoveAll(x => x.tick <= m_currentTick);
            }

            Send();
        }

        bool Listen() {
            if (socket.Available <= 0)
                return false;

            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 11000);
            byte[] data = socket.Receive(ref remoteEP);

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
        }
        private void OnDestroy() {
            byte[] msg = new byte[1 + sizeof(int)];
            msg[0] = (byte)MessageType.DISCONNECT;
            Buffer.BlockCopy(BitConverter.GetBytes(m_iD), 0, msg, 1, sizeof(int));
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

            //TODO: set live data to currentGamestate
            //TODO: make Client Tick

            m_currentTick++;
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
            Client client = clients.Find(x => x.iD == id);

            if (client == null)
                return;

            client.eP = ep;

            uint min = uint.MaxValue;
            int offset = sizeof(int) + sizeof(MessageType);
            while (offset <= data.Length) {
                Input IMelement = new Input();
                offset += IMelement.Decrypt(data, offset);
                if (IMelement.tick > client.maxTick) {
                    client.inputs.Add(IMelement);
                    client.maxTick = IMelement.tick;
                }
                if (min > IMelement.tick)
                    min = IMelement.tick;
            }
            if (min > 0 && client.confirmedTick < min - 1) {
                client.confirmedTick = min - 1;

                client.gameStates.RemoveAll(x => x.tick < client.confirmedTick);
            }
        }
#else
        /// NON:        byte Type, int ID, {uint tick, int size, InputType[] inputs}[] tickInputs
        void Send() {
            int size = sizeof(int) + sizeof(MessageType);
            int pos = size;
            List<byte[]> values = new List<byte[]>();
            foreach (var it in unconfirmedInputs) {
                byte[] element = it.Encrypt();
                size += element.Length;
                values.Add(element);
            }

            byte[] value = new byte[size];
            value[0] = (byte)MessageType.NON;
            Buffer.BlockCopy(BitConverter.GetBytes(m_iD), 0, value, sizeof(MessageType), sizeof(int));

            for (int i = 0; i < values.Count; i++) {
                Buffer.BlockCopy(values[i], 0, value, pos, values[i].Length);
                pos += values[i].Length;
            }

            socket.Send(value, value.Length);
        }
#endif
#if UNITY_SERVER
        /// NON:        byte Type, int Tick, int RefTick, Gamestate[] states
        void Send() {
            foreach(var it in clients) {
                if (!it.isConnected)
                    continue;

                Gamestate currentStateCopy = it.gameStates.Find(x => x.tick == m_currentTick);
                Gamestate confirmedStateCopy = it.gameStates.Find(x => x.tick == it.confirmedTick);
                currentStateCopy.CreateDelta(confirmedStateCopy);

                byte[] enc = currentStateCopy.Encrypt();
                byte[] msg = new byte[enc.Length + 1];
                msg[0] = (byte)MessageType.NON;
                Buffer.BlockCopy(enc, 0, msg, 1, enc.Length);

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
            unconfirmedInputs.RemoveAll(x => x.tick <= element.tick);
        }
#endif
#if UNITY_SERVER
        /// CONNECT:    byte Type
        /// NEWID:      byte Type, int ID
        void HandleConnect(byte[] data, IPEndPoint ep) {
            Client element = new Client();

            element.iD = nextID;
            element.isConnected = true;
            element.maxTick = 0;
            element.confirmedTick = 0;
            element.eP = ep;
            element.inputs = new List<Input>();
            element.gameStates = new List<Gamestate>();

            clients.Add(element);

            byte[] msg = new byte[1 + sizeof(int)];
            msg[0] = (byte)MessageType.NEWID;
            Buffer.BlockCopy(BitConverter.GetBytes(nextID), 0, msg, 1, sizeof(int));
            socket.Send(msg, msg.Length, ep);
            nextID++;
        }

        /// DISCONNECT: byte Type, int ID
        void HandleDisconnect(byte[] data, IPEndPoint ep) {
            int RemoteID = BitConverter.ToInt32(data, 1);
            Client element = clients.Find(x => x.iD == RemoteID);
            if (element != null) {
                element.isConnected = false;
                element.eP = ep;
            }
        }

        /// RECONNECT:  byte Type, int ID
        void HandleReconnect(byte[] data, IPEndPoint ep) {
            int RemoteID = BitConverter.ToInt32(data, 1);
            Client element = clients.Find(x => x.iD == RemoteID);
            if (element != null) {
                element.isConnected = true;
                element.eP = ep;
            } else {
                HandleConnect(data, ep);
            }
        }
#else
        void HandleNewID(byte[] data) {
            m_iD = BitConverter.ToInt32(data, 1);
        }
#endif
    }
}