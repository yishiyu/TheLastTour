using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TheLastTour.Controller.Machine
{
    [RequireComponent(typeof(SphereCollider))]
    public class PartJointController : MonoBehaviour
    {
        private SphereCollider _collider;

        private SphereCollider Collider
        {
            get
            {
                if (_collider == null)
                {
                    _collider = GetComponent<SphereCollider>();
                }

                return _collider;
            }
        }


        private PartController _owner;

        public PartController Owner
        {
            get
            {
                if (_owner == null)
                {
                    _owner = transform.parent.parent.GetComponent<PartController>();
                }

                return _owner;
            }
        }


        private bool _isAttached;

        public bool IsAttached
        {
            get { return _isAttached; }
            private set
            {
                _isAttached = value;
                Collider.enabled = !value;
            }
        }

        private PartJointController _connectedJoint;

        public PartJointController ConnectedJoint
        {
            get { return _connectedJoint; }
            set
            {
                _connectedJoint = value;
                IsAttached = (value != null);
            }
        }


        // 根据连接到的 Joint 位置推算得到的自己应处的位置
        public Vector3 InferredPosition { get; private set; } = Vector3.zero;
        public Quaternion InferredRotation { get; private set; } = Quaternion.identity;

        public void TurnOnJointCollision(bool isOn)
        {
            if (Collider != null)
            {
                Collider.enabled = isOn;
            }
        }

        /// <summary>
        /// 自身在父级 Part 中的 JointId
        /// </summary>
        public int JointIdInPart
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


        /// <summary>
        /// 附着到另一个 Joint 上,包括设置双方状态,根据对方位置推算自己位置.
        /// </summary>
        /// <param name="joint">另一个 joint</param>
        /// <param name="ignoreOthers">是否允许对方再被检测到</param>
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
                // 关闭 joint 的碰撞,就不会在被检测到了
                joint.TurnOnJointCollision(true);
            }

            var connectedTransform = ConnectedJoint.transform;
            InferredPosition = connectedTransform.position;
            InferredRotation = connectedTransform.rotation * Quaternion.Euler(0.0f, 180.0f, 0.0f);
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