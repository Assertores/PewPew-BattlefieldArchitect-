using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace T2 {
    public class FaceTick : MonoBehaviour {

        [SerializeField] GSObject m_exampleObject;

        void Start() {
#if UNITY_SERVER
            LockstepTest2.DoTick += ServerTick;
#endif
        }

        private void OnDestroy() {
#if UNITY_SERVER
            LockstepTest2.DoTick -= ServerTick;
#endif
        }

        void ServerTick(uint tick) {
            

            foreach (var IN in FaceInput.s_refList) {
                Debug.Log("[Server] start calculating tick " + tick + " for client " + IN.m_iD);
                d_Input tmp = IN.GetInputForTick(tick);
                if (tmp == null || tmp.inputs == null)
                    continue;

                foreach (var it in tmp.inputs) {
                    Debug.Log("[Server] client " + IN.m_iD + " has inputed " + (InputType)it);

                    switch ((InputType)it) {
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

            Debug.Log("[Server] example object is now at " + m_exampleObject.transform.position);
        }
    }
}