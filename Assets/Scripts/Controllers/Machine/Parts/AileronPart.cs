using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TheLastTour.Controller.Machine
{
    public class AileronPart : FixedPart
    {
        public GameObject aileronMesh;
        private Quaternion _aileronMeshRotation;

        readonly PropertyValue<Key> _propertyAileronUp = new PropertyValue<Key>(Key.None);
        readonly PropertyValue<Key> _propertyAileronDown = new PropertyValue<Key>(Key.None);
        readonly PropertyValue<float> _propertyBaseLiftCoefficient = new PropertyValue<float>(1);
        readonly PropertyValue<float> _propertyStabilityRotateStep = new PropertyValue<float>(30);

        protected override void InitProperties()
        {
            base.InitProperties();

            Properties.Add(new MachineProperty("Stability Up", _propertyAileronUp));
            Properties.Add(new MachineProperty("Stability Down", _propertyAileronDown));
            Properties.Add(new MachineProperty("Stability", _propertyBaseLiftCoefficient));
            Properties.Add(new MachineProperty("Stability Rotate Step", _propertyStabilityRotateStep));

            _aileronMeshRotation = aileronMesh.transform.localRotation;
        }

        public override void FixedUpdate()
        {
            
            base.FixedUpdate();
            float pitch = 0;

            if (_propertyAileronUp.Value != Key.None)
            {
                if (Keyboard.current[_propertyAileronUp.Value].isPressed)
                {
                    pitch += _propertyStabilityRotateStep.Value;
                }
            }

            if (_propertyAileronDown.Value != Key.None)
            {
                if (Keyboard.current[_propertyAileronDown.Value].isPressed)
                {
                    pitch -= _propertyStabilityRotateStep.Value;
                }
            }


            Quaternion controlRotation = Quaternion.Euler(pitch, 0, 0);

            if (aileronMesh)
            {
                aileronMesh.transform.localRotation = controlRotation * _aileronMeshRotation;
            }


            if (SimulatorRigidbody)
            {
                // 局部速度(局部坐标)
                float speed = Vector3.Dot(transform.forward,
                    SimulatorRigidbody.GetPointVelocity(transform.position));

                float aileronForce = speed * speed * _propertyBaseLiftCoefficient.Value * (1 - math.sin(pitch)) / 100;

                // 允许绕 x 轴自由旋转

                SimulatorRigidbody.AddForceAtPosition(
                    transform.up * aileronForce,
                    transform.position,
                    ForceMode.Impulse);


                Debug.DrawLine(transform.position,
                    transform.position +  transform.up * 10,
                    Color.red);

                Debug.DrawLine(transform.position,
                    transform.position + aileronForce  * transform.up,
                    Color.blue);
            }
        }
    }
}