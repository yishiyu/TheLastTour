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
        /// 将一个新的 Part 添加到自身,包括挂载 Transform 和更新质量
        /// 对于 Machine,将 Part 添加到 machineParts 列表中,将 Part 挂载到自身,更新自身质量
        /// 对于 Fixed Part,将事件转发到上层 Simulator
        /// 对于 Movable Part,将 Part 挂载到自身并添加到 attachedParts 列表中,更新自身质量,同时触发所在上层 Simulator 更新质量
        /// </summary>
        /// <param name="part"></param>
        public void AddPart(PartController part);

        /// <summary>
        /// 将一个已有的 Part 从自身移除,包括解除挂载 Transform 和更新质量
        /// 该函数会在 Part 被销毁时,由自己调用自己
        /// 
        /// 对于 Machine,将 Part 从 machineParts 列表中移除,将 Part 从自身解除挂载,更新自身质量
        /// 对于 Fixed Part,将事件转发到上层 Simulator,保持参数不变(通常是自己)
        /// 对于 Movable Part,将 Part 从 attachedParts 列表中移除,将 Part 从自身解除挂载,更新自身质量,同时触发所在上层 Simulator 更新质量
        ///
        /// 如果 Part 是 Fixed Part,尝试将连接到该物体的所有 Part 分离出去,成为独立的 Machine
        /// 如果 Part 是 Movable Part,尝试将连接到该物体的所有 Part 分离出去,成为独立的 Machine
        /// </summary>
        /// <param name="part"></param>
        public void RemovePart(PartController part);

        /// <summary>
        /// 更新 Simulator 的质量
        /// 对于 Machine,更新自身质量
        /// 对于 Fixed Part,将事件转发到上层 Simulator
        /// 对于 Movable Part,更新自身质量,同时触发所在上层 Simulator 更新质量
        /// </summary>
        public void UpdateMachineMass();
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
        }


        public void AddPart(PartController part)
        {
            machineParts.Add(part);
            part.transform.SetParent(transform);
            UpdateMachineMass();
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

            Destroy(part.gameObject);

            // 该 Machine 已无任何 Part,销毁该 Machine
            TheLastTourArchitecture.Instance.GetManager<IMachineManager>().DestroyMachine(this);
        }

        public MachineController DetachJoint(PartJointController joint)
        {
            if (joint != null && joint.IsAttached)
            {
                // 关键元件损坏后,与其连接的所有其他元件分离出去,成为独立的 Machine
                List<PartController> parts = joint.GetConnectedPartsRecursively();
                MachineController machine =
                    TheLastTourArchitecture.Instance.GetManager<IMachineManager>().CreateEmptyMachine();

                foreach (var part in parts)
                {
                    machineParts.Remove(part);
                    machine.machineParts.Add(part);

                    part.transform.parent = machine.transform;
                }

                joint.Detach();

                UpdateMachineMass();
                machine.UpdateMachineMass();
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

        public void UpdateMachineMass()
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