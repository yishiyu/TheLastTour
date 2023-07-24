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
        private Rigidbody _rigidbody;

        private List<PartController> _parts = new List<PartController>();

        private bool _isInited = false;

        public void Init()
        {
            if (!_isInited)
            {
                _isInited = true;
                _rigidbody = GetComponent<Rigidbody>();
                _parts.AddRange(GetComponentsInChildren<PartController>());
            }
        }

        private void Start()
        {
            Init();
        }

        public void AddPart(PartController part)
        {
            _parts.Add(part);
            UpdateMachineMass();
        }

        public void RemovePart(PartController part)
        {
            if (!_parts.Contains(part))
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
            if (_parts.Count == 0)
            {
                Destroy(gameObject);
            }

            UpdateMachineMass();
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
                    _parts.Remove(part);
                    machine._parts.Add(part);

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
            }
            else
            {
                _rigidbody.useGravity = false;
                _rigidbody.isKinematic = true;
            }
        }

        private void UpdateMachineMass()
        {
            if (_parts.Count == 0)
            {
                return;
            }

            float mass = 0;
            Vector3 centerOfMass = Vector3.zero;
            foreach (var part in _parts)
            {
                mass += part.mass;
                centerOfMass += part.mass * part.transform.position;
            }

            _rigidbody.mass = mass;
            _rigidbody.centerOfMass = centerOfMass / mass;
        }
    }
}