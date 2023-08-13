using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TheLastTour.Controller.Machine
{
    public class StepperMotorPart : MovablePart
    {
        public HingeJoint motorJoint;

        private float _currentAngle = 0;
        private float _maxAngle = 90f;
        private float _minAngle = -90f;

        private Vector3 _rotationBase = Vector3.zero;


        private PropertyValue<Key> _motorTurnLeft = new PropertyValue<Key>(Key.None);
        private PropertyValue<Key> _motorTurnRight = new PropertyValue<Key>(Key.None);
        private PropertyValue<float> _motorStep = new PropertyValue<float>(10f);


        public override void OnAttached(ISimulator simulator)
        {
            base.OnAttached(simulator);

            motorJoint.connectedBody = simulator.GetSimulatorRigidbody();
            motorJoint.enableCollision = true;

            _rotationBase = transform.localRotation.eulerAngles;
        }


        protected override void InitProperties()
        {
            base.InitProperties();

            Properties.Add(new MachineProperty("Motor Left", _motorTurnLeft));
            Properties.Add(new MachineProperty("Motor Right", _motorTurnRight));
            Properties.Add(new MachineProperty("Motor Step", _motorStep));
        }


        public override void Update()
        {
            if (_motorTurnLeft.Value != Key.None)
            {
                if (Keyboard.current[_motorTurnLeft.Value].wasPressedThisFrame)
                {
                    _currentAngle = Mathf.Clamp(_currentAngle - _motorStep.Value, _minAngle, _maxAngle);
                }
            }

            if (_motorTurnRight.Value != Key.None)
            {
                if (Keyboard.current[_motorTurnRight.Value].wasPressedThisFrame)
                {
                    _currentAngle = Mathf.Clamp(_currentAngle + _motorStep.Value, _minAngle, _maxAngle);
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            transform.localRotation = Quaternion.Euler(
                _rotationBase.x,
                _rotationBase.y + _currentAngle,
                _rotationBase.z
            );
            //
            // // 将世界坐标下的旋转转为局部坐标的旋转
            // Vector3 relativeAngularVelocity =
            //     Quaternion.Inverse(transform.rotation) * MovablePartRigidbody.angularVelocity;
            // // 对自身施加力矩
            // float torque = _motorPower.Value * Power -
            //                _motorDamping.Value * relativeAngularVelocity.y;
            //
            // // float torque = _motorPower.Value * Power;
            // MovablePartRigidbody.AddRelativeTorque(torque * Vector3.up);
            // // 对父级施加力矩
            // if (SimulatorRigidbody != null)
            // {
            //     SimulatorRigidbody.AddRelativeTorque(-torque * (transform.localRotation * Vector3.up));
            // }
            //
            // //
            // // MovablePartRigidbody.AddRelativeTorque(torque * Vector3.right);
            // // Debug.Log("Motor Power: " + _motorPower.Value * Power +
            // //           "  Power: " + Power +
            // //           "  _motorPower.Value: " + _motorPower.Value +
            // //           "  _motorDamping.Value: " + _motorDamping.Value +
            // //           "  relativeAngularVelocity.x: " + relativeAngularVelocity.x);
            //
            // Debug.Log("Motor Power: " + _motorPower.Value * Power +
            //           "  Power: " + Power +
            //           "  _motorPower.Value: " + _motorPower.Value +
            //           "  _motorDamping.Value: " + _motorDamping.Value +
            //           "  relativeAngularVelocity: " + relativeAngularVelocity);
        }
    }
}