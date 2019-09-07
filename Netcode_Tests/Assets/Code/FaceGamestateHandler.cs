using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using System;

namespace T2 {

    [StructLayout(LayoutKind.Explicit)]
    public struct GameStateItem {
        [FieldOffset(0)]
        public uint iD;
        [FieldOffset(sizeof(uint))]
        public float health;
        [FieldOffset(sizeof(uint) + sizeof(float))]
        public float posX;
        [FieldOffset(sizeof(uint) + 2 * sizeof(float))]
        public float posY;
        [FieldOffset(sizeof(uint) + 3 * sizeof(float))]
        public float posZ;

        [FieldOffset(0)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = sizeof(uint) + sizeof(float) + 3 * sizeof(float))]
        public byte[] data;
    }

    //should be outsourced to a class
    public struct Gamestate {
        public uint tick;
        public bool isDelta;
        public bool isLerped;
        public uint refTick;
        public List<GameStateItem> states;

        public byte[] Encrypt() {
            int sizeofGameState = sizeof(uint) + sizeof(float) + 3 * sizeof(float);
            byte[] value = new byte[2 * sizeof(uint) + (states == null ? 0 : states.Count * sizeofGameState)];

            Buffer.BlockCopy(BitConverter.GetBytes(tick), 0, value, 0, sizeof(uint));
            Buffer.BlockCopy(BitConverter.GetBytes(refTick), 0, value, sizeof(uint), sizeof(uint));

            if (states != null) {
                for (int i = 0; i < states.Count; i++) {
                    //Buffer.BlockCopy(states[i].data, 0, value, 2 * sizeof(uint) + i * sizeofGameState, sizeofGameState);
                    Buffer.BlockCopy(BitConverter.GetBytes(states[i].iD), 0, value, 2 * sizeof(uint) + i * sizeofGameState, sizeof(uint));
                    Buffer.BlockCopy(BitConverter.GetBytes(states[i].health), 0, value, 2 * sizeof(uint) + i * sizeofGameState + sizeof(uint), sizeof(float));
                    Buffer.BlockCopy(BitConverter.GetBytes(states[i].posX), 0, value, 2 * sizeof(uint) + i * sizeofGameState + sizeof(uint) + sizeof(float), sizeof(float));
                    Buffer.BlockCopy(BitConverter.GetBytes(states[i].posY), 0, value, 2 * sizeof(uint) + i * sizeofGameState + sizeof(uint) + sizeof(float) + sizeof(float), sizeof(float));
                    Buffer.BlockCopy(BitConverter.GetBytes(states[i].posZ), 0, value, 2 * sizeof(uint) + i * sizeofGameState + sizeof(uint) + sizeof(float) + 2 * sizeof(float), sizeof(float));
                }
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

            states = new List<GameStateItem>((msg.Length - offset) / sizeofGameState);
            for (int i = 0; i < states.Count; i++) {
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

            states = newState;
            isDelta = false;
            return true;
        }

        public bool CreateDelta(Gamestate reference) {
            if (reference.states == null)
                return false;
            if (reference.tick >= tick)
                return false;
            if (reference.isDelta)
                return false;

            List<GameStateItem> delta = new List<GameStateItem>();
            foreach (var it in states) {
                int index = -1;
                for (int i = 0; i < reference.states.Count; i++) {
                    if (reference.states[i].iD == it.iD) {
                        index = i;
                        break;
                    }
                }
                if (index == -1 ||
                    it.health != reference.states[index].health ||
                    it.posX != reference.states[index].posX ||
                    it.posY != reference.states[index].posY ||
                    it.posZ != reference.states[index].posZ) {
                    delta.Add(it);
                }
            }

            states = delta;
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
            List<GameStateItem> newState = new List<GameStateItem>();
            if (start.states != null)
                newState.AddRange(start.states);

            foreach (var it in states) {
                if (!newState.Exists(x => x.iD == it.iD) && lv > 0.5f) {
                    newState.Add(it);
                    continue;
                }
                int index = newState.FindIndex(x => x.iD == it.iD);
                GameStateItem element = newState[index];
                element.posX = Mathf.Lerp(element.posX, it.posX, lv);
                element.posY = Mathf.Lerp(element.posY, it.posY, lv);
                element.posZ = Mathf.Lerp(element.posZ, it.posZ, lv);
                element.health = (int)Mathf.Lerp(element.health, it.health, lv);

                newState[index] = element;
            }

            states = newState;

            isDelta = false;
            isLerped = true;
            refTick = start.refTick;
            return true;
        }

        public override string ToString() {
            StringBuilder stg = new StringBuilder();
            stg.Append("Tick: " + tick + ", ");
            stg.Append("isDelta: " + isDelta + ", ");
            stg.Append("isLerped: " + isLerped + ", ");
            stg.Append("reference Tick: " + refTick);

            foreach (var it in states) {
                stg.Append("\n");
                stg.Append("ID: " + it.iD + ", ");
                stg.Append("Health: " + it.health + ", ");
                stg.Append("pos: " + it.posX + ", " + it.posY + ", " + it.posZ);
            }

            return stg.ToString();
        }
    }

    public class FaceGamestateHandler : Singelton<FaceGamestateHandler> {

        public List<GSObject> m_objects = new List<GSObject>();

        public void ApplyGameState(Gamestate state) {
            if (state.isDelta)
                return;

            foreach(var it in state.states) {
                print(it.iD);
                GSObject element = m_objects.Find(x => x.m_iD == it.iD);
                if (!element) {//TODO: somehow new element
                    print("element not found");
                    continue;
                }
                element.m_health = it.health;
                element.transform.position = new Vector3(it.posX, it.posY, it.posZ);
            }
            foreach(var it in m_objects) {
                if(!state.states.Exists(x => x.iD == it.m_iD)) {//TODO: somehow element was removed
                    print("element " + it.m_iD + " not in new gamestate");
                    continue;
                }
            }
        }

        public Gamestate CreateGameState(uint tick) {
            Gamestate value = new Gamestate();
            value.states = new List<GameStateItem>();
            foreach(var it in m_objects) {
                GameStateItem element = new GameStateItem();
                element.iD = it.m_iD;
                element.health = it.m_health;
                element.posX = it.transform.position.x;
                element.posY = it.transform.position.y;
                element.posZ = it.transform.position.z;
                value.states.Add(element);
            }

            value.tick = tick;
            value.isDelta = false;
            value.isLerped = false;
            value.refTick = tick;

            return value;
        }
    }
}