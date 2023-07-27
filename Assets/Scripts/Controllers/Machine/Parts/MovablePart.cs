using System.Collections;
using System.Collections.Generic;
using TheLastTour.Manager;
using UnityEngine;

namespace TheLastTour.Controller.Machine
{
    /// <summary>
    /// MovablePart 是可以移动的 Part,需要添加 Rigidbody 组件
    /// 包括可以主动运动的轮子,传动轴轴,也包括只能被动运动的关节
    /// 其运动由自身的 Rigidbody 组件模拟,同时通过 Joint 与父级 Rigidbody 连接
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class MovablePart : PartController
    {
        private Rigidbody _rigidbody;

        public List<PartController> attachedParts = new List<PartController>();

        public Rigidbody MovablePartRigidbody
        {
            get
            {
                if (_rigidbody == null)
                {
                    _rigidbody = GetComponent<Rigidbody>();
                }

                return _rigidbody;
            }
        }


        public override Rigidbody GetSimulatorRigidbody()
        {
            return MovablePartRigidbody;
        }

        public override void OnAttached(ISimulator simulator)
        {
            Debug.Log("MovablePart OnAttached");
        }

        public override void AddPart(PartController part)
        {
            attachedParts.Add(part);
            part.transform.parent = transform;
            UpdateSimulatorMass();
        }

        public override void RemovePart(PartController part)
        {
            if (!attachedParts.Contains(part) || part.isCorePart)
            {
                return;
            }

            if (part == this)
            {
                transform.parent.GetComponent<ISimulator>().RemovePart(part);
            }
            else
            {
                part.Detach();
                attachedParts.Remove(part);

                // 拆分该 Part 的所有连接,分别形成多个 Machine
                foreach (var joint in part.joints)
                {
                    DetachJoint(joint);
                }
            }

            // 运行到这里,part 一定不是 Movable Part 自身
            // 故不需要销毁自身及所在 Machine
            UpdateSimulatorMass();
        }

        private MachineController DetachJoint(PartJointController joint)
        {
            if (joint != null && joint.IsAttached)
            {
                // 递归搜索所有与该 Joint 相连的 Part
                List<PartController> parts = joint.GetConnectedPartsRecursively();
                MachineController machine =
                    TheLastTourArchitecture.Instance.GetManager<IMachineManager>().CreateEmptyMachine();

                foreach (var part in parts)
                {
                    attachedParts.Remove(part);
                    machine.machineParts.Add(part);

                    part.transform.SetParent(machine.transform);
                    part.OnAttached(machine);
                }

                joint.Detach();

                UpdateSimulatorMass();
                machine.UpdateSimulatorMass();
                return machine;
            }

            return null;
        }

        public override void UpdateSimulatorMass()
        {
            Debug.Log("MovablePart UpdateSimulatorMass");
            transform.parent.GetComponent<ISimulator>().UpdateSimulatorMass();
        }

        public override bool IsLeafNode()
        {
            return true;
        }
    }
}