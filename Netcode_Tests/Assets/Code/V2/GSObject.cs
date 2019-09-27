using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace T2 {
    public class GSObject : MonoBehaviour {

        static uint s_nextID = 0;

        [SerializeField] public float m_maxHealth;
        [HideInInspector] public uint m_iD;
        [HideInInspector] public float m_health;

        private void Start() {
            m_health = m_maxHealth;
            m_iD = s_nextID;
            s_nextID++;
            FaceGamestateHandler.s_singelton.m_objects.Add(this);
        }
        private void OnDestroy() {
			if(FaceGamestateHandler.Exists())
				FaceGamestateHandler.s_singelton.m_objects.Remove(this);
        }
    }
}