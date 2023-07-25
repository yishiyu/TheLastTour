using System;
using System.Collections.Generic;
using System.Linq;
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

        public void TurnOnJointCollision(bool isOn)
        {
            if (_collider != null)
            {
                _collider.enabled = isOn;
            }
        }

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

        public List<PartController> GetConnectedPartsRecursively()
        {
            HashSet<PartController> connectedParts = new HashSet<PartController>();

            HashSet<PartController> temp = new HashSet<PartController>();

            // 防止向自身传播
            connectedParts.Add(Owner);
            // 添加自身所连接的部件
            if (ConnectedJoint != null)
            {
                connectedParts.Add(ConnectedJoint.Owner);
                temp.Add(ConnectedJoint.Owner);
            }

            while (temp.Count > 0)
            {
                PartController part = temp.First();
                temp.Remove(part);

                foreach (PartJointController joint in part.joints)
                {
                    if (joint.ConnectedJoint != null && !connectedParts.Contains(joint.ConnectedJoint.Owner))
                    {
                        connectedParts.Add(joint.ConnectedJoint.Owner);
                        temp.Add(joint.ConnectedJoint.Owner);
                    }
                }
            }

            // 移除自身
            connectedParts.Remove(Owner);


            return connectedParts.ToList();
        }


        private void Awake()
        {
            _collider = GetComponent<SphereCollider>();
            Owner = transform.parent.parent.GetComponent<PartController>();
        }


        public void Attach(PartJointController joint, bool ignoreOthers = true)
        {
            if (IsAttached || joint == null || joint.IsAttached)
            {
                return;
            }

            ConnectedJoint = joint;
            joint.ConnectedJoint = this;

            if (!ignoreOthers)
            {
                joint.TurnOnJointCollision(true);
            }


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