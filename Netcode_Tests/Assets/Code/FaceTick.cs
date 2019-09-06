using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace T2 {
    public class FaceTick : MonoBehaviour {

        [SerializeField] GSObject m_exampleObject;

        void Start() {
#if !UNITY_SERVER
            LockstepTest2.DoTick += ServerTick;
#endif
        }

        private void OnDestroy() {
#if !UNITY_SERVER
            LockstepTest2.DoTick -= ServerTick;
#endif
        }

        void ServerTick(uint tick) {
            foreach(var IN in FaceInput.s_refList) {
                foreach(var it in IN.GetInputForTick(tick).inputs) {
                    switch (it) {
                    case InputType.FORWARD:
                        m_exampleObject.transform.position += new Vector3(Time.fixedDeltaTime, 0, 0);
                        break;
                    case InputType.BACKWARD:
                        m_exampleObject.transform.position -= new Vector3(Time.fixedDeltaTime, 0, 0);
                        break;
                    case InputType.LEFT:
                        m_exampleObject.transform.position += new Vector3(0, Time.fixedDeltaTime, 0);
                        break;
                    case InputType.RIGHT:
                        m_exampleObject.transform.position -= new Vector3(0, Time.fixedDeltaTime, 0);
                        break;
                    case InputType.UP:
                        m_exampleObject.transform.position += new Vector3(0, 0, Time.fixedDeltaTime);
                        break;
                    case InputType.DOWN:
                        m_exampleObject.transform.position -= new Vector3(0, 0, Time.fixedDeltaTime);
                        break;
                    default:
                        break;
                    }
                }
            }
        }
    }
}