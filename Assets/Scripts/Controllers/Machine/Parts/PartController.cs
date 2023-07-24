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

        private PartJointController _rootJoint = null;

        public int RootJointId { get; private set; } = -1;


        public PartJointController ConnectedJoint
        {
            get
            {
                if (_rootJoint)
                {
                    return _rootJoint.ConnectedJoint;
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
                _rootJoint = _joints[id];
            }
        }

        private void Awake()
        {
            foreach (var joint in GetComponentsInChildren<PartJointController>())
            {
                _joints.Add(joint);
            }

            if (!isCorePart && _joints.Count > 0)
            {
                RootJointId = 0;
                _rootJoint = _joints[0];
            }
        }


        public void AttachTo(PartJointController joint)
        {
            if (joint == null || joint.IsAttached || _rootJoint == null)
            {
                return;
            }

            _rootJoint.Attach(joint);
            Vector3 VOffset = _rootJoint.transform.localPosition;
            Quaternion QOffset = _rootJoint.transform.localRotation;
            transform.rotation = _rootJoint.Rotation * Quaternion.Inverse(QOffset);
            transform.position = _rootJoint.Position - transform.rotation * VOffset;


            GetComponentInParent<MachineController>().UpdateMachineMass();
        }

        public void Detach()
        {
            if (_rootJoint != null)
            {
                _rootJoint.Detach();
                _rootJoint = null;
            }

            GetComponentInParent<MachineController>().UpdateMachineMass();
        }
    }
}