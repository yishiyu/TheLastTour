using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TheLastTour.Controller.Machine
{
    public class WheelPart : MovablePart
    {
        public HingeJoint motorJoint;

        private Rigidbody _simulatorRigidbody;

        private Rigidbody SimulatorRigidbody
        {
            get
            {
                if (_simulatorRigidbody == null)
                {
                    _simulatorRigidbody = transform.parent.GetComponentInParent<ISimulator>().GetSimulatorRigidbody();
                }

                return _simulatorRigidbody;
            }
        }


        private PropertyValue<Key> _powerForward = new PropertyValue<Key>(Key.None);
        private PropertyValue<Key> _powerBackward = new PropertyValue<Key>(Key.None);
        private PropertyValue<float> _power = new PropertyValue<float>(100);
        private PropertyValue<float> _damping = new PropertyValue<float>(0.5f);


        public override void OnAttached(ISimulator simulator)
        {
            base.OnAttached(simulator);

            motorJoint.connectedBody = simulator.GetSimulatorRigidbody();
        }


        protected override void InitProperties()
        {
            base.InitProperties();

            _damping.Value = MovablePartRigidbody.angularDrag;

            Properties.Add(new MachineProperty("Forward Key", _powerForward));
            Properties.Add(new MachineProperty("Backward Key", _powerBackward));
            Properties.Add(new MachineProperty("Power", _power));
            Properties.Add(new MachineProperty("Damping", _damping));
        }


        public override void FixedUpdate()
        {
            base.FixedUpdate();

            float torque = 0;
            if (_powerForward.Value != Key.None)
            {
                if (Keyboard.current[_powerForward.Value].isPressed)
                {
                    torque = _power.Value;
                }
            }

            if (_powerBackward.Value != Key.None)
            {
                if (Keyboard.current[_powerBackward.Value].isPressed)
                {
                    torque = -_power.Value;
                }
            }

            // 对自身施加力矩
            MovablePartRigidbody.AddRelativeTorque(torque * Vector3.forward);
            // 对父级施加力矩
            if (SimulatorRigidbody != null)
            {
                SimulatorRigidbody.AddRelativeTorque(-torque * (transform.localRotation * Vector3.forward));
            }
            // 水平作用力由约束自动完成


            // Debug.Log("Motor Power: " + _motorPower.Value * Power +
            //           "  Power: " + Power +
            //           "  _motorPower.Value: " + _motorPower.Value +
            //           "  _motorDamping.Value: " + _motorDamping.Value +
            //           "  torque: " + torque);
        }
    }
}