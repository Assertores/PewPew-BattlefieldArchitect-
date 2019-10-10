using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour{

    static T m_singleton = default;
    public static T s_singleton {
        get {
            if (m_singleton == null) {
                Debug.Log("no object of type " + typeof(T) + " was found.");
                GameObject tmp = new GameObject("SINGLETON_" + typeof(T));
                m_singleton = tmp.AddComponent<T>();
            }
            return m_singleton;
        }
    }

    private void Awake() {
        if (m_singleton != null) {
            Debug.Log("multible instances of type " + typeof(T));
            Destroy(this);
            return;
        }
        m_singleton = this as T;
    }
}
