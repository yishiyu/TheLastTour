using System;
using System.Collections.Generic;
using System.Linq;
using TheLastTour.Event;
using TheLastTour.Manager;
using UnityEngine;

namespace TheLastTour.Controller.Machine
{
    [RequireComponent(typeof(Rigidbody))]
    public class MachineController : MonoBehaviour
    {
        public List<PartController> machineParts = new List<PartController>();

        private Rigidbody _rigidbody;
        private bool _initialized = false;

        public void Init()
        {
            // 新创建的 Machine,可能在 Start 前就进行了操作,需要立即初始化
            // 同时防止重复初始化
            if (!_initialized)
            {
                _initialized = true;
                _rigidbody = GetComponent<Rigidbody>();
                machineParts.AddRange(GetComponentsInChildren<PartController>());
            }
        }

        private void Start()
        {
            Init();
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

            _rigidbody.mass = mass;
            _rigidbody.centerOfMass = centerOfMass / mass;
        }
    }
}