using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.VFX;

namespace TheLastTour.Controller.Machine
{
    public class RocketPart : FixedPart
    {
        public ParticleSystem visualEffect;

        readonly PropertyValue<float> _propertyPower = new PropertyValue<float>(1000);
        readonly PropertyValue<float> _propertyFuel = new PropertyValue<float>(3);
        readonly PropertyValue<Key> _propertyTrigger = new PropertyValue<Key>(Key.None);

        bool _isTriggered = false;


        protected override void InitProperties()
        {
            base.InitProperties();

            Properties.Add(new MachineProperty("Power", _propertyPower));
            Properties.Add(new MachineProperty("Fuel", _propertyFuel));
            Properties.Add(new MachineProperty("Trigger", _propertyTrigger));
        }

        private void Update()
        {
            if (_propertyTrigger.Value != Key.None && !_isTriggered)
            {
                if (Keyboard.current[_propertyTrigger.Value].wasPressedThisFrame)
                {
                    _isTriggered = true;
                    // 播放特效
                    visualEffect.Play();
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (_isTriggered && SimulatorRigidbody)
            {
                _propertyFuel.Value -= Time.fixedDeltaTime;
                if (_propertyFuel.Value <= 0)
                {
                    _propertyFuel.Value = 0;
                    _isTriggered = false;
                    visualEffect.Stop();
                    return;
                }

                Vector3 force = _propertyPower.Value * transform.up;

                SimulatorRigidbody.AddForceAtPosition(
                    force,
                    transform.position,
                    ForceMode.Impulse);
            }
        }
    }
}