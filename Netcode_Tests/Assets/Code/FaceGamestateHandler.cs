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
        public Vector3 pos;

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
            int sizeofGameState = sizeof(uint) + 3 * sizeof(float);
            byte[] value = new byte[2 * sizeof(uint) + states.Count * sizeofGameState];

            Buffer.BlockCopy(BitConverter.GetBytes(tick), 0, value, 0, sizeof(uint));
            Buffer.BlockCopy(BitConverter.GetBytes(refTick), 0, value, sizeof(uint), sizeof(uint));

            for (int i = 0; i < states.Count; i++) {
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
                    it.pos != reference.states[index].pos) {
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

            states = newState;

            isDelta = false;
            isLerped = true;
            refTick = start.refTick;
            return true;
        }
    }

    public class FaceGamestateHandler : Singelton<FaceGamestateHandler> {

        public List<GSObject> m_objects = new List<GSObject>();

        public void ApplyGameState(Gamestate state) {
            if (state.isDelta)
                return;

            foreach(var it in state.states) {
                GSObject element = m_objects.Find(x => x.m_iD == it.iD);
                if (!element) {//TODO: somehow new element
                    continue;
                }
                element.m_health = it.health;
                element.transform.position = it.pos;
            }
            foreach(var it in m_objects) {
                if(!state.states.Exists(x => x.iD == it.m_iD)) {//TODO: somehow element was removed
                    continue;
                }
            }
        }

        public Gamestate CreateGameState(uint tick) {
            Gamestate value = new Gamestate();
            value.tick = tick;
            value.isDelta = false;
            value.isLerped = false;
            value.refTick = tick;
            foreach(var it in m_objects) {
                GameStateItem element = new GameStateItem();
                element.iD = it.m_iD;
                element.health = it.m_health;
                element.pos = it.transform.position;
                value.states.Add(element);
            }
            return value;
        }
    }
}