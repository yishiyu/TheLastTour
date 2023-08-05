using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheLastTour.Controller.Machine
{
    public class GyroPart : FixedPart
    {
        Rigidbody _rigidBody;

        Rigidbody SimulatorRigidBody
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

        readonly PropertyValue<float> _propertyAngleStability = new PropertyValue<float>(1f);
        readonly PropertyValue<float> _propertyAngularVelocityStability = new PropertyValue<float>(0.5f);
        readonly PropertyValue<float> _propertyAngleDeadZone = new PropertyValue<float>(10f);
        readonly PropertyValue<float> _propertyAngularVelocityDeadZone = new PropertyValue<float>(2f);
        private float _initRotation;

        protected override void InitProperties()
        {
            base.InitProperties();

            Properties.Add(new MachineProperty("Angle Stability", _propertyAngleStability));
            Properties.Add(new MachineProperty("Angular Velocity Stability", _propertyAngularVelocityStability));
            Properties.Add(new MachineProperty("Dead Zone", _propertyAngleDeadZone));
            Properties.Add(new MachineProperty("Angular Velocity Dead Zone", _propertyAngularVelocityDeadZone));


            _initRotation = transform.rotation.eulerAngles.z;
        }

        public override void OnAttached(ISimulator simulator)
        {
            base.OnAttached(simulator);

            _initRotation = transform.rotation.eulerAngles.z;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (SimulatorRigidBody)
            {
                // 根据角度计算力矩,力矩只与角度符号有关
                float angle = _initRotation - transform.rotation.eulerAngles.z;

                // 归一化到 -180 ~ 180
                angle = (angle - 360 * Mathf.FloorToInt((angle + 180f) / 360f));
                if (Mathf.Abs(angle) < _propertyAngleDeadZone.Value)
                {
                    angle = 0;
                }
                else
                {
                    angle = angle > 0 ? 1 : -1;
                }


                // 根据角速度修正力矩,防止出现抖动
                float angularVelocity = transform.InverseTransformDirection(SimulatorRigidBody.angularVelocity).z;

                if (Mathf.Abs(angularVelocity) < _propertyAngularVelocityDeadZone.Value)
                {
                    angularVelocity = 0;
                }
                else
                {
                    angularVelocity = angularVelocity > 0 ? 1 : -1;
                }


                // Debug.DrawLine(transform.position, transform.position - transform.forward * angularVelocity,
                //     Color.red);
                // Debug.DrawLine(transform.position, transform.position + transform.forward * angle, Color.blue);

                float torque = angle * _propertyAngleStability.Value -
                               angularVelocity * _propertyAngularVelocityStability.Value;

                // Debug.Log(
                //     "angle:" + angle + "\n" +
                //     "angularVelocity:" + angularVelocity + "\n" +
                //     "torque:" + torque
                // );

                // Vector3 direction = transform.forward;
                SimulatorRigidBody.AddRelativeTorque(
                    transform.forward * torque,
                    ForceMode.Impulse);
                //
                Debug.DrawLine(transform.position, transform.position + transform.forward * 10, Color.red);
                Debug.DrawLine(transform.position, transform.position + transform.forward * torque, Color.blue);
            }
        }
    }
}