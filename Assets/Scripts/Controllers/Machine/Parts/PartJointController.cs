using System;
using UnityEngine;

namespace TheLastTour.Controller.Machine
{
    [RequireComponent(typeof(SphereCollider))]
    public class PartJointController : MonoBehaviour
    {
        private SphereCollider _collider;
        private bool _isAttached = false;
        private PartJointController _connectedJoint = null;


        public bool IsAttached
        {
            get { return _isAttached; }
            private set
            {
                _isAttached = value;
                _collider.enabled = !value;
            }
        }

        public PartJointController ConnectedJoint
        {
            get { return _connectedJoint; }
            set
            {
                _connectedJoint = value;
                IsAttached = (value != null);
            }
        }

        public PartController Owner { get; private set; } = null;

        public Vector3 Position { get; private set; } = Vector3.zero;

        public Quaternion Rotation { get; private set; } = Quaternion.identity;


        public int JoointId
        {
            get
            {
                if (Owner != null)
                {
                    return Owner.GetJointId(this);
                }

                return -1;
            }
        }


        private void Awake()
        {
            _collider = GetComponent<SphereCollider>();
            Owner = transform.parent.GetComponent<PartController>();
        }


        public void Attach(PartJointController joint)
        {
            if (IsAttached || joint == null || joint.IsAttached)
            {
                return;
            }

            ConnectedJoint = joint;
            joint.ConnectedJoint = this;

            Position = ConnectedJoint.transform.position;
            Rotation = ConnectedJoint.transform.rotation * Quaternion.Euler(0.0f, 180.0f, 0.0f);
        }

        public void Detach()
        {
            if (!IsAttached)
            {
                return;
            }

            if (ConnectedJoint != null)
            {
                ConnectedJoint.ConnectedJoint = null;
            }

            ConnectedJoint = null;
        }
    }
}