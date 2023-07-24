using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheLastTour.Controller.Machine
{
    public class PartController : MonoBehaviour
    {
        // 可设置属性
        public string partName = "Part";
        public float mass = 10;

        public bool isCorePart = false;

        // 不同组件连接相关属性
        // 与更接近核心组件连接的接口,决定该组件的位置和旋转
        private List<PartJointController> _joints = new List<PartJointController>();

        public PartJointController rootJoint = null;

        public int RootJointId { get; private set; } = -1;


        public PartJointController ConnectedJoint
        {
            get
            {
                if (rootJoint)
                {
                    return rootJoint.ConnectedJoint;
                }

                return null;
            }
        }

        public int GetJointId(PartJointController joint)
        {
            for (int i = 0; i < _joints.Count; i++)
            {
                if (_joints[i] == joint)
                {
                    return i;
                }
            }

            return -1;
        }

        public PartJointController GetJointById(int id)
        {
            if (id > 0 && id < _joints.Count)
            {
                return _joints[id];
            }

            return null;
        }

        public void SetRootJoint(int id)
        {
            if (id > 0 && id < _joints.Count)
            {
                RootJointId = id;
                rootJoint = _joints[id];
            }
        }

        private void Awake()
        {
            foreach (var joint in GetComponentsInChildren<PartJointController>())
            {
                _joints.Add(joint);
            }

            if (rootJoint != null)
            {
                RootJointId = GetJointId(rootJoint);
                if (RootJointId < 0 || RootJointId > _joints.Count)
                {
                    rootJoint = null;
                }
            }
            else if (!isCorePart && _joints.Count > 0)
            {
                RootJointId = 0;
                rootJoint = _joints[0];
            }
        }


        public void AttachTo(PartJointController joint)
        {
            if (joint == null || joint.IsAttached || rootJoint == null)
            {
                return;
            }

            rootJoint.Attach(joint);
            Vector3 VOffset = rootJoint.transform.localPosition;
            Quaternion QOffset = rootJoint.transform.localRotation;
            transform.rotation = rootJoint.Rotation * Quaternion.Inverse(QOffset);
            transform.position = rootJoint.Position - transform.rotation * VOffset;
        }

        public void Detach()
        {
            if (rootJoint != null)
            {
                rootJoint.Detach();
            }
        }
    }
}