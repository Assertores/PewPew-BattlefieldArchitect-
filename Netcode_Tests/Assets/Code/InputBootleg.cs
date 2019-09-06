using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace T2 {
    public class InputBootleg : MonoBehaviour {
        // Start is called before the first frame update
        void Start() {
#if !UNITY_SERVER
            gameObject.AddComponent<FaceInput>();
#endif
            Destroy(this);
        }
    }
}