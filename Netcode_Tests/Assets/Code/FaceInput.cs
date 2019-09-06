using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace T2 {

    public enum InputType : byte { FORWARD, BACKWARD, LEFT, RIGHT, UP, DOWN }

    public struct d_Input {
        public uint tick;
        public List<InputType> inputs;

        public byte[] Encrypt() {
            byte[] value = new byte[2 * sizeof(uint) + inputs.Count * sizeof(InputType)];
            Buffer.BlockCopy(BitConverter.GetBytes(tick), 0, value, 0, sizeof(uint));
            Buffer.BlockCopy(BitConverter.GetBytes(inputs.Count), 0, value, sizeof(uint), sizeof(int));
            Buffer.BlockCopy(inputs.ToArray(), 0, value, sizeof(uint) + sizeof(int), inputs.Count * sizeof(InputType));
            return value;
        }

        public int Decrypt(byte[] msg, int offset) {
            offset += sizeof(uint) + sizeof(int);
            tick = BitConverter.ToUInt32(msg, offset);
            int size = BitConverter.ToInt32(msg, offset + sizeof(uint));

            inputs = new List<InputType>(size);

            Buffer.BlockCopy(msg, offset, inputs.ToArray(), 0, size * sizeof(InputType));
            return offset + size * sizeof(InputType);
        }
    }

    public class FaceInput : MonoBehaviour {

        public static List<FaceInput> s_refList = new List<FaceInput>();

        public int m_iD = -1;
        uint m_buffersize = 6;

        List<d_Input> m_inputQueue { get; private set; } = new List<d_Input>();
        
        void Start() {
            s_refList.Add(this);
        }

        private void OnDestroy() {
            s_refList.Remove(this);
        }
        
        void Update() {
            if (Input.GetKey(KeyCode.W))
                m_inputQueue[m_inputQueue.Count - 1].inputs.Add(InputType.FORWARD);

            if (Input.GetKey(KeyCode.A))
                m_inputQueue[m_inputQueue.Count - 1].inputs.Add(InputType.LEFT);

            if (Input.GetKey(KeyCode.S))
                m_inputQueue[m_inputQueue.Count - 1].inputs.Add(InputType.BACKWARD);

            if (Input.GetKey(KeyCode.D))
                m_inputQueue[m_inputQueue.Count - 1].inputs.Add(InputType.RIGHT);
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
        }
        
        public void AddNewElement(d_Input tick) {
            if (m_inputQueue.Exists(x => x.tick == tick.tick))
                return;

            m_inputQueue.Add(tick);
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