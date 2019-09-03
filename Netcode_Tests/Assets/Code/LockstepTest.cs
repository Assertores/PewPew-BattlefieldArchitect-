using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// ===== ===== Package Layout ===== =====
/// every packages first byte descripes the package type (MessageType)
/// Client -> Server:
/// NON:        byte Type, int ID, {uint tick, int size, InputType[] inputs}[] tickInputs
/// CONNECT:    byte Type
/// DISCONNECT: byte Type, int ID
/// RECONNECT:  byte Type, int ID
/// CONFIRM:    byte Type, uint Tick
/// Server -> Client:
/// NON:        byte Type, int Tick, int RefTick, Gamestate[] states
/// NEWID:      byte Type, int ID
/// ===== ===== GameState buffer ===== =====
/// server buffers all gamestates of all players upto the last confirmed tick, the server resived from that client
/// client buffers all gamestates                upto the last gamestate the server used as references
/// </summary>
//TODO: mal in memorystream rein schauen

enum MessageType : byte { NON, CONNECT, DISCONNECT, RECONNECT, NEWID, CONFIRM }

enum InputType : byte { FORWARD, BACKWARD, LEFT, RIGHT, UP, DOWN }

[StructLayout(LayoutKind.Explicit)]
struct InputMessageItem {
    [FieldOffset(0)]
    public uint tick;
    [FieldOffset(sizeof(uint))]
    public InputType type;

    [FieldOffset(0)]
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = sizeof(uint) + sizeof(InputType))]
    public byte[] data;
}

[StructLayout(LayoutKind.Explicit)]
struct GameStateMessageItem {
    [FieldOffset(0)]
    public uint tick;
    [FieldOffset(sizeof(uint))]
    public uint iD;
    [FieldOffset(2 * sizeof(uint))]
    public Vector3 pos;

    [FieldOffset(0)]
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2 * sizeof(uint) + 3 * sizeof(float))]
    public byte[] data;
}

struct InputMessage {
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
        tick = BitConverter.ToUInt32(msg, 0);
        int size = BitConverter.ToInt32(msg, sizeof(uint));

        inputs = new InputType[size];

        Buffer.BlockCopy(msg, offset, inputs, 0, size * sizeof(InputType));
        return offset + size * sizeof(InputType);
    }
}

[StructLayout(LayoutKind.Explicit)]
struct GameStateItem {
    [FieldOffset(0)]
    public uint iD;
    [FieldOffset(sizeof(uint))]
    public Vector3 pos;

    [FieldOffset(0)]
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = sizeof(uint) + 3 * sizeof(float))]
    public byte[] data;
}

struct GameStateMessage {
    public uint tick;
    public GameStateItem[] states;

    public byte[] Encrypt() {
        int sizeofGameState = sizeof(uint) + 3 * sizeof(float);
        byte[] value = new byte[sizeof(uint) + states.Length * sizeofGameState];

        Buffer.BlockCopy(BitConverter.GetBytes(tick), 0, value, 0, sizeof(uint));

        for (int i = 0; i < states.Length; i++) {
            Buffer.BlockCopy(states[i].data, 0, value, sizeof(uint) + i * sizeofGameState, sizeofGameState);
        }

        return value;
    }

    public void Decrypt(byte[] msg, int offset) {
        int sizeofGameState = sizeof(uint) + 3 * sizeof(float);
        tick = BitConverter.ToUInt32(msg, offset);
        offset += sizeof(uint);

        states = new GameStateItem[(msg.Length - offset) / sizeofGameState];
        for (int i = 0; i < states.Length; i++) {
            Buffer.BlockCopy(msg, offset + i * sizeofGameState, states[i].data, 0, sizeofGameState);
        }
    }
}

public class LockstepTest : MonoBehaviour {

    public static Action<uint> DoTick;

    uint m_currentTick = 0;
    UdpClient socket;
    
    public int m_serverPort = 11000;

    int m_sizeofInputMessageItem = Marshal.SizeOf<InputMessageItem>();
    int m_sizeofGameStateMessageItem = Marshal.SizeOf<GameStateMessageItem>();

#if UNITY_SERVER

    //array von allen inputs queues mit id und lastIPEndPoint und maxtickimput
    class inputQueueElement {//weil c# dum ist und mir eine kopie des structs zurück giebt anstadt eine reference auf das struct
        public int iD;
        public IPEndPoint eP;
        public uint maxTick;
        public List<InputMessage> inputs;
    }

    List<inputQueueElement> inputs = new List<inputQueueElement>();
    GameStateMessage currentGameState;

    int nextID = 0;

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
        foreach(var it in inputs) {
            if (min > it.maxTick)
                min = it.maxTick;
        }
        if (min <= m_currentTick)
            return;

        //simulate

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
            int clientID = BitConverter.ToInt32(data, sizeof(MessageType));
            int index = inputs.FindIndex(x => x.iD == clientID);
            //TODO: was wenn es den index nicht gibt?

            inputs[index].eP = remoteEP;

            int offset = sizeof(int) + sizeof(MessageType);
            while(offset <= data.Length) {
                InputMessage IMelement = new InputMessage();
                offset += IMelement.Decrypt(data, offset);
                if(IMelement.tick > inputs[index].maxTick) {
                    inputs[index].inputs.Add(IMelement);
                    inputs[index].maxTick = IMelement.tick;
                }
            }
            break;
        case MessageType.CONNECT:
            inputQueueElement IQEelement = new inputQueueElement();
            IQEelement.eP = remoteEP;
            IQEelement.iD = nextID;
            nextID++;
            IQEelement.maxTick = 0;
            IQEelement.inputs = new List<InputMessage>();
            inputs.Add(IQEelement);

            byte[] msg = new byte[1 + sizeof(int)];
            msg[0] = (byte)MessageType.NEWID;
            Buffer.BlockCopy(BitConverter.GetBytes(IQEelement.iD), 0, msg, 1, sizeof(int));
            socket.Send(msg, msg.Length);
            break;
        case MessageType.DISCONNECT:
            break;
        case MessageType.RECONNECT:
            break;
        default:
            break;
        }

        return true;
    }

    void Send() {
        foreach(var it in inputs) {
            byte[] msg = currentGameState.Encrypt();
            socket.Send(msg, msg.Length, it.eP);
        }
    }

#else

    IPEndPoint ep;
    public string m_iP = "127.0.0.1";

    public uint m_confirmedTick = 0;

    public int m_iD = -1;

    List<GameStateMessage> m_gameStates = new List<GameStateMessage>();//unordert
    List<InputMessage> m_input = new List<InputMessage>();

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
        byte[] msg = new byte[1];
        msg[0] = (byte)MessageType.DISCONNECT;
        socket.Send(msg, msg.Length);

        socket.Close();
    }

    void Update() {
        Listen();
    }

    private void FixedUpdate() {

        //set gamestate on live data

        Send();
    }

    void Listen() {
        if (socket.Available <= 0)
            return;

        byte[] data = socket.Receive(ref ep);

        MessageType messageType = (MessageType)data[0];

        switch (messageType) {
        case MessageType.NEWID:
            m_iD = BitConverter.ToInt32(data, 1);
            break;
        case MessageType.NON:
            GameStateMessage element = new GameStateMessage();
            element.Decrypt(data, sizeof(MessageType));
            SetConfirmedTick(element.tick);
            m_gameStates.Add(element);
            break;
        default:
            break;
        }
    }

    void Send() {
        int size = sizeof(int) + sizeof(MessageType);
        int pos = size;
        List<byte[]> values = new List<byte[]>();
        for (uint i = m_confirmedTick + 1; i < m_currentTick; i++) {
            byte[] element = m_input.Find(x => x.tick == i).Encrypt();
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

    void SetConfirmedTick(uint value) {
        if (value < m_confirmedTick)
            return;

        m_confirmedTick = value;

        m_input.RemoveAll(x => x.tick <= value);
    }

#endif
}
