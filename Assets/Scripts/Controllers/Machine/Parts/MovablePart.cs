using System;
using System.Collections;
using System.Collections.Generic;
using TheLastTour.Manager;
using Unity.Mathematics;
using UnityEngine;

namespace TheLastTour.Controller.Machine
{
    /// <summary>
    /// MovablePart 是可以移动的 Part,需要添加 Rigidbody 组件
    /// 包括可以主动运动的轮子,传动轴轴,也包括只能被动运动的关节
    /// 其运动由自身的 Rigidbody 组件模拟,同时通过 Joint 与父级 Rigidbody 连接
    /// </summary>
    public class MovablePart : PartController
    {
        public List<PartController> attachedParts = new List<PartController>();

        public Rigidbody movablePartRigidbody;


        private Rigidbody _parentRigidbody;

        public Rigidbody ParentRigidbody
        {
            get
            {
                if (_parentRigidbody == null && transform.parent != null)
                {
                    _parentRigidbody = transform.parent.GetComponentInParent<ISimulator>().GetSimulatorRigidbody();
                }

                return _parentRigidbody;
            }
        }


        public override float Mass
        {
            get
            {
                // if (MovablePartRigidbody != null)
                // {
                //     return MovablePartRigidbody.mass;
                // }
                // Movable 独立负责自己的重力,其父级不需要控制该物体
                return 0;
            }
        }

        public override Vector3 CenterOfMass
        {
            get
            {
                if (SimulatorRigidbody != null)
                {
                    return SimulatorRigidbody.centerOfMass;
                }

                return Vector3.zero;
            }
        }


        public override Rigidbody GetSimulatorRigidbody()
        {
            if (movablePartRigidbody)
            {
                return movablePartRigidbody;
            }

            return GetComponent<Rigidbody>();
        }

        public override void OnAttached(ISimulator simulator)
        {
            Debug.Log("MovablePart OnAttached");
        }

        public override void AddPart(PartController part)
        {
            attachedParts.Add(part);
            part.transform.parent = transform;
            part.OnAttached(this);
            UpdateSimulatorMass();
        }

        public override void RemovePart(PartController part)
        {
            if (part == this)
            {
                transform.parent.GetComponent<ISimulator>().RemovePart(part);
            }

            if (!attachedParts.Contains(part) || part.isCorePart)
            {
                return;
            }

            part.Detach();
            attachedParts.Remove(part);

            // 拆分该 Part 的所有连接,分别形成多个 Machine
            foreach (var joint in part.joints)
            {
                DetachJoint(joint);
            }

            // 运行到这里,part 一定不是 Movable Part 自身
            // 故不需要销毁自身及所在 Machine,但是需要销毁传入的 Part
            Destroy(part.gameObject);
            UpdateSimulatorMass();
        }

        public override ISimulator DetachJoint(PartJointController joint)
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
            float totalMass = mass;
            Vector3 massCenter = centerOfMass;
            // 正方体本身有 0.667 的惯性惯量
            // 用平行轴定理计算质心相对于自身的惯性惯量
            float intertiaX = mass * (centerOfMass.y * centerOfMass.y + centerOfMass.z * centerOfMass.z + 0.667f);
            float intertiaY = mass * (centerOfMass.x * centerOfMass.x + centerOfMass.z * centerOfMass.z + 0.667f);
            float intertiaZ = mass * (centerOfMass.x * centerOfMass.x + centerOfMass.y * centerOfMass.y + 0.667f);

            Quaternion inversedRotation = Quaternion.Inverse(SimulatorRigidbody.rotation);
            foreach (var part in attachedParts)
            {
                // part 的质心相对于自身的偏移
                // 手动算坐标，否则会受到 Rigidbody scale 的影响
                // There are three similar methods on transform: TransformPoint, TransformDirection, and TransformVector. The difference comes down to which aspects of the transform are used or not used when performing the transform:
                //
                // TransformPoint 6: position, rotation, and scale
                // TransformDirection 5: rotation only
                // TransformVector 6: rotation and scale only
                // Vector3 partMassPosition = SimulatorRigidbody.transform.InverseTransformPoint(part.transform.TransformPoint(part.centerOfMass));
                Vector3 partMassPosition = part.transform.position + part.transform.rotation * part.CenterOfMass;
                partMassPosition = inversedRotation * (partMassPosition - SimulatorRigidbody.position);
                totalMass += part.Mass;
                massCenter += part.Mass * partMassPosition;
                intertiaX += part.Mass * (partMassPosition.y * partMassPosition.y +
                                          partMassPosition.z * partMassPosition.z + 0.6667f);
                intertiaY += part.Mass * (partMassPosition.x * partMassPosition.x +
                                          partMassPosition.z * partMassPosition.z + 0.6667f);
                intertiaZ += part.Mass * (partMassPosition.x * partMassPosition.x +
                                          partMassPosition.y * partMassPosition.y + 0.6667f);
            }

            SimulatorRigidbody.mass = totalMass;
            SimulatorRigidbody.centerOfMass = massCenter / totalMass;
            SimulatorRigidbody.inertiaTensor = new Vector3(intertiaX, intertiaY, intertiaZ);

            // transform.parent.GetComponent<ISimulator>().UpdateSimulatorMass();
        }

        public override bool IsLeafNode()
        {
            return false;
        }

        private void OnDrawGizmos()
        {
            if (IsDrawGizmos)
            {
                Gizmos.color = new Color(0, 1, 0, 0.5f);
                Gizmos.DrawSphere(SimulatorRigidbody.worldCenterOfMass, math.sqrt(SimulatorRigidbody.mass) / 10f);
            }
        }

        public override void TurnOnSimulation(bool isOn)
        {
            base.TurnOnSimulation(isOn);

            // MovablePartRigidbody.useGravity = false;
            SimulatorRigidbody.isKinematic = !isOn;


            foreach (var part in attachedParts)
            {
                part.TurnOnSimulation(isOn);
            }
        }
    }
}