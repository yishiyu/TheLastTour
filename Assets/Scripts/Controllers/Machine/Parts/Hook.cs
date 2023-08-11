using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheLastTour.Controller.Machine
{
    public class Hook : MonoBehaviour
    {
        public Rigidbody hookRigidbody;
        public GameObject hookedGameObject;
        public FixedJoint fixedJoint;

        public bool isHooked = false;


        public void Release()
        {
            if (hookedGameObject)
            {
                Destroy(fixedJoint);
                fixedJoint = null;
                hookedGameObject = null;
            }

            isHooked = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Hookable"))
            {
                var rb = other.attachedRigidbody;
                if (!isHooked && rb)
                {
                    isHooked = true;
                    hookedGameObject = other.gameObject;
                    fixedJoint = hookedGameObject.AddComponent<FixedJoint>();
                    fixedJoint.connectedBody = hookRigidbody;
                    fixedJoint.connectedAnchor = Vector3.zero;
                    fixedJoint.autoConfigureConnectedAnchor = true;
                }
            }
        }
    }
}