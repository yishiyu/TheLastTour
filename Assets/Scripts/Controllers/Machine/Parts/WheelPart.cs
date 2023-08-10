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
        public GameObject arrowModel;

        private PropertyValue<Key> _powerForward = new PropertyValue<Key>(Key.None);
        private PropertyValue<Key> _powerBackward = new PropertyValue<Key>(Key.None);
        private PropertyValue<float> _power = new PropertyValue<float>(100);
        private PropertyValue<float> _damping = new PropertyValue<float>(0.5f);

        public override void TurnOnSimulation(bool isOn)
        {
            base.TurnOnSimulation(isOn);

            if (isOn)
            {
                arrowModel.SetActive(false);
            }
            else
            {
                arrowModel.SetActive(true);
            }
        }


        public override void OnAttached(ISimulator simulator)
        {
            base.OnAttached(simulator);

            motorJoint.connectedBody = simulator.GetSimulatorRigidbody();
            motorJoint.enableCollision = true;
        }


        protected override void InitProperties()
        {
            base.InitProperties();

            _damping.Value = SimulatorRigidbody.angularDrag;

            Properties.Add(new MachineProperty("Forward Key", _powerForward));
            Properties.Add(new MachineProperty("Backward Key", _powerBackward));
            Properties.Add(new MachineProperty("Power", _power));
            Properties.Add(new MachineProperty("Damping", _damping));
        }


        public override void FixedUpdate()
        {
            base.FixedUpdate();

            float torqueValue = 0;
            if (_powerForward.Value != Key.None)
            {
                if (Keyboard.current[_powerForward.Value].isPressed)
                {
                    torqueValue = _power.Value;
                }
            }

            if (_powerBackward.Value != Key.None)
            {
                if (Keyboard.current[_powerBackward.Value].isPressed)
                {
                    torqueValue = -_power.Value;
                }
            }

            Vector3 torque = torqueValue * transform.forward;

            // 对自身施加力矩
            SimulatorRigidbody.AddTorque(torque);
            // 对父级施加力矩
            if (ParentRigidbody != null)
            {
                ParentRigidbody.AddTorque(-torque);
            }
            // 水平作用力由约束自动完成


            // Debug.Log("torque: " + torque);
        }
    }
}