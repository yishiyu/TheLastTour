using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TheLastTour.Controller.Machine
{
    public class WheelProPart : FixedPart
    {
        private PropertyValue<Key> _powerForward = new PropertyValue<Key>(Key.None);
        private PropertyValue<Key> _powerBackward = new PropertyValue<Key>(Key.None);
        private PropertyValue<Key> _turnLeft = new PropertyValue<Key>(Key.None);
        private PropertyValue<Key> _turnRight = new PropertyValue<Key>(Key.None);
        private PropertyValue<float> _power = new PropertyValue<float>(100);
        private PropertyValue<float> _turnAngle = new PropertyValue<float>(10);
        private PropertyValue<float> _damping = new PropertyValue<float>(0.5f);

        public WheelCollider wheelCollider;
        public GameObject wheelModel;
        public GameObject arrowModel;

        public override void TurnOnSimulation(bool isOn)
        {
            base.TurnOnSimulation(isOn);

            if (isOn)
            {
                arrowModel.SetActive(false);
                // wheelModel.GetComponent<Collider>().enabled = false;
            }
            else
            {
                arrowModel.SetActive(true);
                // wheelModel.GetComponent<Collider>().enabled = true;
            }
        }


        protected override void InitProperties()
        {
            base.InitProperties();

            Properties.Add(new MachineProperty("Forward Key", _powerForward));
            Properties.Add(new MachineProperty("Backward Key", _powerBackward));
            Properties.Add(new MachineProperty("Turn Left Key", _turnLeft));
            Properties.Add(new MachineProperty("Turn Right Key", _turnRight));
            Properties.Add(new MachineProperty("Power", _power));
            Properties.Add(new MachineProperty("Turn Angle", _turnAngle));
            Properties.Add(new MachineProperty("Damping", _damping));

            wheelCollider.mass = _massProperty.Value;
            wheelCollider.wheelDampingRate = _damping.Value;

            _massProperty.OnValueChanged += (f => { wheelCollider.mass = f; });
            _damping.OnValueChanged += (f => { wheelCollider.wheelDampingRate = f; });
        }

        public void UpdateWheelMeshPosition()
        {
            wheelCollider.GetWorldPose(out Vector3 pos, out Quaternion rot);
            Vector3 rotation = (Quaternion.Inverse(transform.rotation) * rot).eulerAngles;
            // Debug.Log("rotation: " + rotation);
            float yaw = rotation.y - Mathf.FloorToInt((rotation.y + 90) / 180) * 180;
            // float roll = rotation.z - Mathf.FloorToInt((rotation.z + 90) / 180) * 180;
            rotation = new Vector3(rotation.x, yaw, 0);
            // Debug.Log("processed rotation: " + rotation);
            // Vector3 rotation = new Vector3(rot.eulerAngles.x, 0, rot.eulerAngles.z);

            wheelModel.transform.position = pos;
            wheelModel.transform.localRotation = Quaternion.Euler(rotation);
        }


        public override void FixedUpdate()
        {
            base.FixedUpdate();

            float torque = 0;
            float steerAngle = 0;
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

            if (_turnLeft.Value != Key.None)
            {
                if (Keyboard.current[_turnLeft.Value].isPressed)
                {
                    steerAngle = _turnAngle.Value;
                }
            }

            if (_turnRight.Value != Key.None)
            {
                if (Keyboard.current[_turnRight.Value].isPressed)
                {
                    steerAngle = -_turnAngle.Value;
                }
            }

            // 对自身和父级施加力矩
            wheelCollider.motorTorque = torque;
            wheelCollider.steerAngle = steerAngle;
            if (SimulatorRigidbody != null)
            {
                // 削弱一点对父级的力矩
                SimulatorRigidbody.AddRelativeTorque(-torque * 0.8f * (transform.localRotation * Vector3.forward));
            }

            UpdateWheelMeshPosition();

            // // 对自身施加力矩
            // MovablePartRigidbody.AddRelativeTorque(torque * Vector3.forward);
            // // 对父级施加力矩
            // if (SimulatorRigidbody != null)
            // {
            //     // 削弱一点对父级的力矩
            //     SimulatorRigidbody.AddRelativeTorque(-torque * 0.3f * (transform.localRotation * Vector3.forward));
            // }
            // // 水平作用力由约束自动完成


            // Debug.Log("Motor Power: " + _motorPower.Value * Power +
            //           "  Power: " + Power +
            //           "  _motorPower.Value: " + _motorPower.Value +
            //           "  _motorDamping.Value: " + _motorDamping.Value +
            //           "  torque: " + torque);
        }
    }
}