using System;
using System.Collections.Generic;
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


        private void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        public void AddPart(PartController part)
        {
            _parts.Add(part);
            UpdateMachineMass();
        }

        public void RemovePart(PartController part)
        {
            _parts.Remove(part);
            UpdateMachineMass();
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