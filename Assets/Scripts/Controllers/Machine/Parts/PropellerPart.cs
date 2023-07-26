using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using TheLastTour.Manager;
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


        readonly PropertyValue<Key> _propertyPowerUp = new PropertyValue<Key>(Key.None);
        readonly PropertyValue<Key> _propertyPowerDown = new PropertyValue<Key>(Key.None);
        readonly PropertyValue<float> _propertyMaxPower = new PropertyValue<float>(10);

        public GameObject propellerMesh;

        private IGameStateManager _gameStateManager;

        protected override void InitProperties()
        {
            base.InitProperties();

            _gameStateManager = TheLastTourArchitecture.Instance.GetManager<IGameStateManager>();

            Properties.Add(new MachineProperty("Power Up", _propertyPowerUp));
            Properties.Add(new MachineProperty("Power Down", _propertyPowerDown));
            Properties.Add(new MachineProperty("Max Power", _propertyMaxPower));
        }

        private void Update()
        {
            if (_gameStateManager == null || _gameStateManager.GameState != EGameState.Play)
            {
                return;
            }

            if (_propertyPowerUp.Value != Key.None)
            {
                if (Keyboard.current[_propertyPowerUp.Value].wasPressedThisFrame)
                {
                    Power += 0.1f;
                }
            }

            if (_propertyPowerDown.Value != Key.None)
            {
                if (Keyboard.current[_propertyPowerDown.Value].wasPressedThisFrame)
                {
                    Power -= 0.1f;
                }
            }

            propellerMesh.transform.Rotate(
                0,
                0,
                300 * Power * _propertyMaxPower.Value * Time.deltaTime,
                Space.Self);
        }

        private void FixedUpdate()
        {
            if (RigidBody)
            {
                RigidBody.AddForceAtPosition(
                    transform.up * (Power * _propertyMaxPower.Value),
                    transform.position,
                    ForceMode.Impulse);
            }
        }
    }
}