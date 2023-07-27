using System;
using System.Collections.Generic;
using System.Linq;
using TheLastTour.Event;
using TheLastTour.Manager;
using UnityEngine;

namespace TheLastTour.Controller.Machine
{
    public interface ISimulator
    {
        /// <summary>
        /// 获取控制该物体的 Rigidbody
        /// 对于 MovablePart,返回自身的 Rigidbody
        /// 对于 FixedPart,返回父级的 Rigidbody
        /// 对于 Machine,返回自身的 Rigidbody
        /// </summary>
        /// <returns></returns>
        Rigidbody GetSimulatorRigidbody();

        /// <summary>
        /// 获取控制该物体的 MachineController
        /// </summary>
        /// <returns></returns>
        MachineController GetOwnerMachine();

        /// <summary>
        /// 该 Simulator 连接到了新的主物体上,更新自身的信息
        /// 包括一些约束的对象切换,质量更新等
        /// </summary>
        /// <param name="simulator"></param>
        void OnAttached(ISimulator simulator);

        /// <summary>
        /// 将一个新的 Part 添加到自身,包括挂载 Transform 和更新质量
        /// 对于 Machine,将 Part 添加到 machineParts 列表中,将 Part 挂载到自身,更新自身质量
        /// 对于 Fixed Part,将事件转发到上层 Simulator
        /// 对于 Movable Part,将 Part 挂载到自身并添加到 attachedParts 列表中,更新自身质量,同时触发所在上层 Simulator 更新质量
        /// </summary>
        /// <param name="part"></param>
        public void AddPart(PartController part);

        /// <summary>
        /// 将一个已有的 Part 从自身移除,包括解除挂载 Transform 和更新质量
        /// 该函数需要在 Part 销毁前调用,同时该函数不会销毁 Part 所在 GameObject
        /// 
        /// 对于 Machine, 尝试将所有与 Part 连接的物体分离出去(成为独立的Machine),然后销毁自身
        /// (无论参数是 Fixed 的还是 Movable 的都是合理的)
        /// (分离出去时,对于被分离的 Fixed Part 没有影响,对于被分离的 Movable Part,更新其约束所连接的 Rigidbody 为新的 Machine 的 Rigidbody
        /// 对于 Fixed Part,将事件转发到上层 Simulator,保持参数不变(通常是自己)
        /// 对于 Movable Part,如果 Part 是自身,则向上转发,保持参数不变; 否则先断开自身与该 Part 与父级的连接,然后尝试将所有与 Part 连接的物体分离出去(成为独立的Machine)
        /// </summary>
        /// <param name="part">被移除的对象,只有是该 Simulator 自身或直接子节点才有效(通常是自身调用并将自身作为参数)</param>
        public void RemovePart(PartController part);

        /// <summary>
        /// 更新 Simulator 的质量
        /// 对于 Machine,更新自身质量
        /// 对于 Fixed Part,将事件转发到上层 Simulator
        /// 对于 Movable Part,更新自身质量,同时触发所在上层 Simulator 更新质量
        /// </summary>
        public void UpdateSimulatorMass();
    }


    [RequireComponent(typeof(Rigidbody))]
    public class MachineController : MonoBehaviour, ISimulator
    {
        public List<PartController> machineParts = new List<PartController>();

        private Rigidbody _rigidbody;

        public Rigidbody MachineRigidBody
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

        private void Start()
        {
            machineParts.AddRange(GetComponentsInChildren<PartController>());
            UpdateSimulatorMass();
        }


        public void OnAttached(ISimulator simulator)
        {
            Debug.LogError("MachineController.OnAttached() is not implemented");
        }

        public void AddPart(PartController part)
        {
            machineParts.Add(part);
            part.transform.SetParent(transform);
            part.OnAttached(this);
            UpdateSimulatorMass();
        }

        public void RemovePart(PartController part)
        {
            if (!machineParts.Contains(part) || part.isCorePart)
            {
                return;
            }

            // 拆分该 Part 的所有连接,分别形成多个 Machine
            foreach (var joint in part.joints)
            {
                DetachJoint(joint);
            }

            machineParts.Remove(part);

            // 该 Machine 已无任何 Part,销毁该 Machine
            TheLastTourArchitecture.Instance.GetManager<IMachineManager>().DestroyMachine(this);
        }

        public MachineController DetachJoint(PartJointController joint)
        {
            if (joint != null && joint.IsAttached)
            {
                // 递归搜索所有与该 Joint 相连的 Part
                List<PartController> parts = joint.GetConnectedPartsRecursively();
                MachineController machine =
                    TheLastTourArchitecture.Instance.GetManager<IMachineManager>().CreateEmptyMachine();

                foreach (var part in parts)
                {
                    machineParts.Remove(part);
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

        public void TurnOnSimulation(bool isOn)
        {
            if (isOn)
            {
                _rigidbody.useGravity = true;
                _rigidbody.isKinematic = false;
                foreach (var part in machineParts)
                {
                    part.TurnOnJointCollision(false);
                }
            }
            else
            {
                _rigidbody.useGravity = false;
                _rigidbody.isKinematic = true;
                foreach (var part in machineParts)
                {
                    part.TurnOnJointCollision(true);
                }
            }
        }

        public void UpdateSimulatorMass()
        {
            if (machineParts.Count == 0)
            {
                return;
            }

            float mass = 0;
            Vector3 centerOfMass = Vector3.zero;
            foreach (var part in machineParts)
            {
                mass += part.mass;
                centerOfMass += part.mass * part.transform.position;
            }

            MachineRigidBody.mass = mass;
            MachineRigidBody.centerOfMass = centerOfMass / mass;
        }

        public Rigidbody GetSimulatorRigidbody()
        {
            return MachineRigidBody;
        }

        public MachineController GetOwnerMachine()
        {
            return this;
        }
    }
}