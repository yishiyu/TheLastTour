using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Unity.Properties;
using UnityEngine;
using UnityEngine.InputSystem;
using Vector3 = UnityEngine.Vector3;

namespace TheLastTour.Controller.Machine
{
    public class PropellerPart : FixedPart
    {
        float _power = 0;
        float _maxPower = 1;

        float Power
        {
            get { return _power; }
            set { _power = Mathf.Clamp(value, 0, _maxPower); }
        }

        Rigidbody _rigidBody;

        Rigidbody RigidBody
        {
            get
            {
                if (_rigidBody == null)
                {
                    _rigidBody = GetComponentInParent<Rigidbody>();
                }

                return _rigidBody;
            }
        }


        PropertyValue<Key> _propertyPowerUp = new PropertyValue<Key>(Key.None);
        PropertyValue<Key> _propertyPowerDown = new PropertyValue<Key>(Key.None);
        PropertyValue<float> _propertyMaxPower = new PropertyValue<float>(10);

        public GameObject propellerMesh;


        protected override void InitProperties()
        {
            base.InitProperties();

            Properties.Add(new MachineProperty("Power Up", _propertyPowerUp));
            Properties.Add(new MachineProperty("Power Down", _propertyPowerDown));
            Properties.Add(new MachineProperty("Max Power", _propertyMaxPower));
        }

        private void Update()
        {
            if (_propertyPowerUp.Value != Key.None)
            {
                if (Keyboard.current[_propertyPowerUp.Value].wasPressedThisFrame)
                {
                    Debug.Log("Power Up");
                    Power += 0.1f;
                }
            }

            if (_propertyPowerDown.Value != Key.None)
            {
                if (Keyboard.current[_propertyPowerDown.Value].wasPressedThisFrame)
                {
                    Debug.Log("Power Down");
                    Power -= 0.1f;
                }
            }

            // propellerMesh.transform.Rotate(
            //     transform.forward,
            //     10 * Power * _propertyMaxPower.Value * Time.deltaTime);
            propellerMesh.transform.Rotate(
                0,
                0,
                100 * Power * _propertyMaxPower.Value * Time.deltaTime,
                Space.Self);
            Debug.Log("Power: " + Power +
                      " MaxPower: " + _propertyMaxPower.Value +
                      " Force: " + Power * _propertyMaxPower.Value);
        }

        private void FixedUpdate()
        {
            if (RigidBody)
            {
                RigidBody.AddForceAtPosition(
                    transform.up * (Power * _propertyMaxPower.Value),
                    transform.position,
                    ForceMode.Impulse);
                Debug.Log("AddForceAtPosition");
            }
        }
    }
}