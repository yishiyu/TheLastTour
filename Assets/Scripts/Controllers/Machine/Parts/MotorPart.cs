using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TheLastTour.Controller.Machine
{
    public class MotorPart : MovablePart
    {
        public HingeJoint motorJoint;

        private float _power = 0;
        private float _maxPower = 1;

        private float Power
        {
            get { return _power; }
            set { _power = Mathf.Clamp(value, -_maxPower, _maxPower); }
        }

        private PropertyValue<Key> _motorPowerUp = new PropertyValue<Key>(Key.None);
        private PropertyValue<Key> _motorPowerDown = new PropertyValue<Key>(Key.None);
        private PropertyValue<float> _motorPower = new PropertyValue<float>(100);
        private PropertyValue<float> _motorDamping = new PropertyValue<float>(100f);


        public override void OnAttached(ISimulator simulator)
        {
            base.OnAttached(simulator);

            motorJoint.connectedBody = simulator.GetSimulatorRigidbody();
        }


        protected override void InitProperties()
        {
            base.InitProperties();

            Properties.Add(new MachineProperty("Motor Key", _motorPowerUp));
            Properties.Add(new MachineProperty("Motor Key", _motorPowerDown));
            Properties.Add(new MachineProperty("Motor Power", _motorPower));
            Properties.Add(new MachineProperty("Motor Damping", _motorDamping));
        }


        private void Update()
        {
            if (_motorPowerUp.Value != Key.None)
            {
                if (Keyboard.current[_motorPowerUp.Value].wasPressedThisFrame)
                {
                    Power += 0.1f;
                }
            }

            if (_motorPowerDown.Value != Key.None)
            {
                if (Keyboard.current[_motorPowerDown.Value].wasPressedThisFrame)
                {
                    Power -= 0.1f;
                }
            }
        }

        private void FixedUpdate()
        {
            float torque = _motorPower.Value * Power - _motorDamping.Value * MovablePartRigidbody.angularVelocity.x;

            MovablePartRigidbody.AddRelativeTorque(torque * Vector3.right);
            Debug.Log("Motor Power: " + _motorPower.Value * Power +
                      "  Power: " + Power +
                      "  _motorPower.Value: " + _motorPower.Value +
                      "  _motorDamping.Value: " + _motorDamping.Value);
        }
    }
}