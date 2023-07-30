using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace TheLastTour.Controller.Machine
{
    public class StabilizerPart : FixedPart
    {
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

        public GameObject stabilizerMesh;
        private Quaternion _stabilizerMeshRotation;

        readonly PropertyValue<Key> _propertyStabilityUp = new PropertyValue<Key>(Key.None);
        readonly PropertyValue<Key> _propertyStabilityDown = new PropertyValue<Key>(Key.None);
        readonly PropertyValue<Key> _propertyStabilityLeft = new PropertyValue<Key>(Key.None);
        readonly PropertyValue<Key> _propertyStabilityRight = new PropertyValue<Key>(Key.None);
        readonly PropertyValue<float> _propertyStability = new PropertyValue<float>(5);
        readonly PropertyValue<float> _propertyStabilityRotateStep = new PropertyValue<float>(30);

        protected override void InitProperties()
        {
            base.InitProperties();

            Properties.Add(new MachineProperty("Stability Up", _propertyStabilityUp));
            Properties.Add(new MachineProperty("Stability Down", _propertyStabilityDown));
            Properties.Add(new MachineProperty("Stability Left", _propertyStabilityLeft));
            Properties.Add(new MachineProperty("Stability Right", _propertyStabilityRight));
            Properties.Add(new MachineProperty("Stability", _propertyStability));
            Properties.Add(new MachineProperty("Stability Rotate Step", _propertyStabilityRotateStep));

            _stabilizerMeshRotation = stabilizerMesh.transform.localRotation;
        }

        private void FixedUpdate()
        {
            float yaw = 0;
            float roll = 0;

            if (_propertyStabilityUp.Value != Key.None)
            {
                if (Keyboard.current[_propertyStabilityUp.Value].isPressed)
                {
                    yaw += _propertyStabilityRotateStep.Value;
                }
            }

            if (_propertyStabilityDown.Value != Key.None)
            {
                if (Keyboard.current[_propertyStabilityDown.Value].isPressed)
                {
                    yaw -= _propertyStabilityRotateStep.Value;
                }
            }

            if (_propertyStabilityLeft.Value != Key.None)
            {
                if (Keyboard.current[_propertyStabilityLeft.Value].isPressed)
                {
                    roll += _propertyStabilityRotateStep.Value;
                }
            }

            if (_propertyStabilityRight.Value != Key.None)
            {
                if (Keyboard.current[_propertyStabilityRight.Value].isPressed)
                {
                    roll -= _propertyStabilityRotateStep.Value;
                }
            }

            Quaternion controlRotation = Quaternion.Euler(0, yaw, roll);

            if (stabilizerMesh)
            {
                stabilizerMesh.transform.localRotation = controlRotation * _stabilizerMeshRotation;
            }


            if (RigidBody)
            {
                controlRotation = transform.localRotation * controlRotation *
                                  Quaternion.Inverse(transform.localRotation);

                // 局部速度(局部坐标)
                float velocity = -Vector3.Dot(controlRotation * transform.forward,
                                     RigidBody.GetPointVelocity(transform.position)) *
                                 Vector3.Dot(controlRotation * transform.right,
                                     RigidBody.GetPointVelocity(transform.position));
                // float velocity = transform.right * RigidBody.GetPointVelocity(transform.position);

                float stabilityForce = velocity * _propertyStability.Value / 100;

                // 允许绕 x 轴自由旋转
                // 绕 y,z 轴旋转 会受到较大回正力矩
                RigidBody.AddForceAtPosition(
                    controlRotation * transform.right * stabilityForce,
                    transform.position,
                    ForceMode.Impulse);

                Debug.DrawLine(
                    transform.position,
                    transform.position + (Vector3.Dot(controlRotation * transform.forward,
                        RigidBody.GetPointVelocity(transform.position))) * (controlRotation * transform.right),
                    Color.cyan
                );

                Debug.DrawLine(
                    transform.position,
                    transform.position + (Vector3.Dot(controlRotation * transform.right,
                        RigidBody.GetPointVelocity(transform.position))) * (controlRotation * transform.right),
                    Color.cyan
                );

                Debug.DrawLine(transform.position,
                    transform.position + controlRotation * transform.right * 10,
                    Color.red);
                Debug.DrawLine(transform.position,
                    transform.position + stabilityForce * (controlRotation * transform.right),
                    Color.blue);

                Debug.Log("velocity: " + velocity +
                          " _propertyStability.Value: " + _propertyStability.Value +
                          " stability force: " + (velocity * _propertyStability.Value));
            }
        }
    }
}