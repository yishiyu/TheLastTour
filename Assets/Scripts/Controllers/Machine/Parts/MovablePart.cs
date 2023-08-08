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
    public class MovablePart : PartController
    {
        public Rigidbody movablePartRigidbody;

        public List<PartController> attachedParts = new List<PartController>();

        public Rigidbody MovablePartRigidbody
        {
            get
            {
                if (movablePartRigidbody == null)
                {
                    movablePartRigidbody = GetComponent<Rigidbody>();
                }

                return movablePartRigidbody;
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
                if (MovablePartRigidbody != null)
                {
                    return MovablePartRigidbody.centerOfMass;
                }

                return Vector3.zero;
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
            float totalMass = mass;
            Vector3 massCenter = centerOfMass;
            float intertiaX = mass * (centerOfMass.y * centerOfMass.y + centerOfMass.z * centerOfMass.z);
            float intertiaY = mass * (centerOfMass.x * centerOfMass.x + centerOfMass.z * centerOfMass.z);
            float intertiaZ = mass * (centerOfMass.x * centerOfMass.x + centerOfMass.y * centerOfMass.y);
            foreach (var part in attachedParts)
            {
                // part 的质心相对于自身的偏移
                Vector3 partMassPosition = part.transform.localPosition + part.CenterOfMass;
                totalMass += part.Mass;
                massCenter += part.Mass * partMassPosition;
                intertiaX += part.Mass * (partMassPosition.y * partMassPosition.y +
                                          partMassPosition.z * partMassPosition.z);
                intertiaY += part.Mass * (partMassPosition.x * partMassPosition.x +
                                          partMassPosition.z * partMassPosition.z);
                intertiaZ += part.Mass * (partMassPosition.x * partMassPosition.x +
                                          partMassPosition.y * partMassPosition.y);
            }

            MovablePartRigidbody.mass = totalMass;
            MovablePartRigidbody.centerOfMass = massCenter / totalMass;
            MovablePartRigidbody.inertiaTensor = new Vector3(intertiaX, intertiaY, intertiaZ) * 10;

            // transform.parent.GetComponent<ISimulator>().UpdateSimulatorMass();
        }

        public override bool IsLeafNode()
        {
            return false;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0, 1, 0, 0.5f);
            Gizmos.DrawSphere(MovablePartRigidbody.worldCenterOfMass, MovablePartRigidbody.mass / 10f);
        }

        public override void TurnOnSimulation(bool isOn)
        {
            base.TurnOnSimulation(isOn);

            // MovablePartRigidbody.useGravity = false;
            MovablePartRigidbody.isKinematic = !isOn;


            foreach (var part in attachedParts)
            {
                part.TurnOnSimulation(isOn);
            }
        }
    }
}