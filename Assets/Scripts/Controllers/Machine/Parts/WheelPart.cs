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
        public float audioVolume = 0.3f;
        public AudioSource audioSource;
        public AudioClip audioClip;
        private float _currentAudioVolume = 0;
        private float _maxAudioVolume = 0.5f;

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

            _maxAudioVolume = Mathf.Clamp(_power.Value / 8000, 0, audioVolume);
            _power.OnValueChanged += (f) => { _maxAudioVolume = Mathf.Clamp(f / 8000, 0, audioVolume); };


            audioSource.clip = audioClip;
            audioSource.loop = true;
            audioSource.volume = 0;

            audioSource.Play();
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

            if (torqueValue > 0.1f)
            {
                _currentAudioVolume = Mathf.Lerp(_currentAudioVolume, _maxAudioVolume, 0.1f);
                audioSource.volume = _currentAudioVolume;
            }
            else if (torqueValue < -0.1f)
            {
                _currentAudioVolume = Mathf.Lerp(_currentAudioVolume, _maxAudioVolume, 0.1f);
                audioSource.volume = _currentAudioVolume;
            }
            else
            {
                _currentAudioVolume = Mathf.Lerp(_currentAudioVolume, 0f, 0.1f);
                audioSource.volume = _currentAudioVolume;
            }


            Vector3 torque = torqueValue * transform.forward;

            // 对自身施加力矩
            SimulatorRigidbody.AddTorque(torque);
            // 对父级施加力矩
            if (ParentRigidbody != null)
            {
                // 减少对父级力矩
                ParentRigidbody.AddTorque(-torque * 0.2f);
            }
            // 水平作用力由约束自动完成


            // Debug.Log("torque: " + torque);
        }
    }
}