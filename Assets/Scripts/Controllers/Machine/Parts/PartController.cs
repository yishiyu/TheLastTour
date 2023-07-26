using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TheLastTour.Controller.Machine
{
    public class PartController : MonoBehaviour
    {
        // 可以设置的属性
        public List<MachineProperty> Properties = new List<MachineProperty>();

        // 可设置属性
        public string partName = "Part";
        public float mass = 10;

        public bool isCorePart = false;

        // 不同组件连接相关属性
        // 与更接近核心组件连接的接口,决定该组件的位置和旋转
        public List<PartJointController> joints = new List<PartJointController>();

        public PartJointController rootJoint = null;

        public int RootJointId { get; private set; } = -1;

        public MachineController GetOwnedMachine()
        {
            return GetComponentInParent<MachineController>();
        }


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
            for (int i = 0; i < joints.Count; i++)
            {
                if (joints[i] == joint)
                {
                    return i;
                }
            }

            return -1;
        }

        public PartJointController GetJointById(int id)
        {
            if (id > 0 && id < joints.Count)
            {
                return joints[id];
            }

            return null;
        }

        public void SetRootJoint(int id)
        {
            if (id > 0 && id < joints.Count)
            {
                RootJointId = id;
                rootJoint = joints[id];
            }
        }

        private void Awake()
        {
            InitProperties();

            foreach (var joint in GetComponentsInChildren<PartJointController>())
            {
                joints.Add(joint);
            }

            if (rootJoint != null)
            {
                RootJointId = GetJointId(rootJoint);
                if (RootJointId < 0 || RootJointId > joints.Count)
                {
                    rootJoint = null;
                }
            }
            else if (!isCorePart && joints.Count > 0)
            {
                RootJointId = 0;
                rootJoint = joints[0];
            }
        }

        protected virtual void InitProperties()
        {
            // var pMass = new PropertyValue<float>(mass);
            // pMass.OnValueChanged += f =>
            // {
            //     mass = f;
            //     GetOwnedMachine().UpdateMachineMass();
            // };
            // Properties.Add(new MachineProperty("test float", pMass));
        }


        public void Attach(PartJointController joint, bool ignoreOthers = true)
        {
            if (joint == null || joint.IsAttached || rootJoint == null)
            {
                return;
            }

            rootJoint.Attach(joint, ignoreOthers);
            // Vector3 VOffset = rootJoint.transform.position - transform.position;
            // Quaternion QOffset = rootJoint.transform.rotation * Quaternion.Inverse(transform.rotation);
            // Vector3 VOffset = rootJoint.transform.localPosition;
            // Quaternion QOffset = rootJoint.transform.localRotation;
            // Joint 的相对偏移, 旋转
            Vector3 VOffset = rootJoint.transform.parent.localRotation * rootJoint.transform.localPosition +
                              rootJoint.transform.parent.localPosition;
            Quaternion QOffset = rootJoint.transform.localRotation * rootJoint.transform.parent.localRotation;
            transform.rotation = rootJoint.Rotation * Quaternion.Inverse(QOffset);
            transform.position = rootJoint.Position - transform.rotation * VOffset;
        }

        public void Detach()
        {
            if (rootJoint != null && rootJoint.IsAttached)
            {
                rootJoint.Detach();
            }
        }

        public void TurnOnJointCollision(bool isOn)
        {
            foreach (var joint in joints)
            {
                joint.TurnOnJointCollision(isOn);
            }
        }
    }
}