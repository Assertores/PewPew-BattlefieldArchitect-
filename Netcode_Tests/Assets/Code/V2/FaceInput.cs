using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace T2 {

    public enum InputType : byte { FORWARD, BACKWARD, LEFT, RIGHT, UP, DOWN }

    public class d_Input {
        public uint tick;
        public List<byte> inputs = new List<byte>();

        public byte[] Encrypt() {
            if(inputs == null || inputs.Count == 0) {
                byte[] tmp = new byte[sizeof(uint) + sizeof(int)];
                Buffer.BlockCopy(BitConverter.GetBytes(tick), 0, tmp, 0, sizeof(uint));
                Buffer.BlockCopy(BitConverter.GetBytes(0), 0, tmp, sizeof(uint), sizeof(int));
                return tmp;
            }
            byte[] value = new byte[sizeof(uint) + sizeof(int) + inputs.Count * sizeof(InputType)];
            Buffer.BlockCopy(BitConverter.GetBytes(tick), 0, value, 0, sizeof(uint));
            Buffer.BlockCopy(BitConverter.GetBytes(inputs.Count), 0, value, sizeof(uint), sizeof(int));
            Buffer.BlockCopy(inputs.ToArray(), 0, value, sizeof(uint) + sizeof(int), inputs.Count * sizeof(InputType));
            return value;
        }

        public int Decrypt(byte[] msg, int offset) {
            tick = BitConverter.ToUInt32(msg, offset);
            offset += sizeof(uint);
            int size = BitConverter.ToInt32(msg, offset);
            offset += sizeof(int);

            byte[] tmp = new byte[size];
            if(size > 0)
                Buffer.BlockCopy(msg, offset, tmp, 0, size);

            inputs = new List<byte>(tmp);

            return offset + size * sizeof(InputType);
        }
    }

    public class FaceInput : MonoBehaviour {

        public static List<FaceInput> s_refList = new List<FaceInput>();

        public int m_iD = -1;
        uint m_buffersize = 6;
        [SerializeField] bool onlyClientSide = false;

        List<d_Input> m_inputQueue = new List<d_Input>();
        d_Input m_currentInputElement = new d_Input();
        
        void Start() {
#if UNITY_SERVER
            if (onlyClientSide) {
                Destroy(this);
                return;
            }
#endif
            s_refList.Add(this);
        }

        private void OnDestroy() {
            s_refList.Remove(this);
        }
        
        void Update() {
            if (Input.GetKey(KeyCode.W))
                m_currentInputElement.inputs.Add((byte)InputType.FORWARD);

            if (Input.GetKey(KeyCode.A))
                m_currentInputElement.inputs.Add((byte)InputType.LEFT);

            if (Input.GetKey(KeyCode.S))
                m_currentInputElement.inputs.Add((byte)InputType.BACKWARD);

            if (Input.GetKey(KeyCode.D))
                m_currentInputElement.inputs.Add((byte)InputType.RIGHT);
        }

        public d_Input GetInputForTick(uint tick) {
            return m_inputQueue.Find(x => x.tick == tick);
        }

        public void CreateNewElement(uint tick) {
            if (m_inputQueue.Exists(x => x.tick == tick))
                return;

            d_Input element = new d_Input();
            element.tick = tick + m_buffersize;
            m_inputQueue.Add(element);
            m_currentInputElement = element;
        }
        
        public void AddNewElement(d_Input tick) {
            if (m_inputQueue.Exists(x => x.tick == tick.tick))
                return;

            m_inputQueue.Add(tick);
            m_currentInputElement = tick;
        }

        public byte[] Encrypt() {
            int size = sizeof(int) + sizeof(MessageType);
            int pos = size;
            List<byte[]> values = new List<byte[]>();
            foreach (var it in m_inputQueue) {
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

            return value;
        }

        public void DequeueUptoTick (uint tick) {
            m_inputQueue.RemoveAll(x => x.tick <= tick);
        }
    }
}