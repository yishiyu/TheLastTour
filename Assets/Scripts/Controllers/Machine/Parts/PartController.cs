using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TheLastTour.Controller.Machine
{
    public class PartController : MonoBehaviour
    {
        // 可设置属性
        public string partName = "Part";
        public float mass = 10;
        public bool isCorePart = false;

        public List<PartJointController> joints = new List<PartJointController>();
        public PartJointController rootJoint = null;

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

        // 可以设置的属性
        public readonly List<MachineProperty> Properties = new List<MachineProperty>();

        public int RootJointId { get; private set; } = -1;

        public void SetRootJoint(int id)
        {
            if (id > 0 && id < joints.Count)
            {
                RootJointId = id;
                rootJoint = joints[id];
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

        public MachineController GetOwnedMachine()
        {
            return GetComponentInParent<MachineController>();
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
        }


        public void Attach(PartJointController joint, bool ignoreOthers = true)
        {
            if (joint == null || joint.IsAttached || rootJoint == null)
            {
                return;
            }

            // 通过根组件与其连接
            rootJoint.Attach(joint, ignoreOthers);

            // 计算当前根组件与 rootJoint 的相对位置
            var rootJointTransform = rootJoint.transform;
            var rootModelTransform = rootJointTransform.parent;
            Vector3 rootJointRelativePosition = rootModelTransform.localRotation * rootJointTransform.localPosition +
                                                rootModelTransform.localPosition;
            Quaternion rootJointRelativeRotation = rootJointTransform.localRotation * rootModelTransform.localRotation;

            // 根据 rootJoint 的位置反推当前组件的位置
            transform.rotation = rootJoint.InferredRotation * Quaternion.Inverse(rootJointRelativeRotation);
            transform.position = rootJoint.InferredPosition - transform.rotation * rootJointRelativePosition;
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