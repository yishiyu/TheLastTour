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

        public bool IsHooked
        {
            get { return hookedGameObject != null; }
        }

        private bool _isEnabled = false;

        public bool IsEnabled
        {
            get => _isEnabled;
            set { _isEnabled = value; }
        }


        public void Release()
        {
            if (hookedGameObject)
            {
                Destroy(fixedJoint);
                fixedJoint = null;
                hookedGameObject = null;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Hookable"))
            {
                var rb = other.attachedRigidbody;
                if (_isEnabled && rb)
                {
                    Release();

                    hookedGameObject = other.gameObject;
                    fixedJoint = hookedGameObject.AddComponent<FixedJoint>();
                    fixedJoint.connectedBody = hookRigidbody;
                    fixedJoint.connectedAnchor = Vector3.zero;
                    fixedJoint.autoConfigureConnectedAnchor = true;

                    _isEnabled = false;
                }
            }
        }
    }
}