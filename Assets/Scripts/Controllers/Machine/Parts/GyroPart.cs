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
        private float _initRotation;

        protected override void InitProperties()
        {
            base.InitProperties();

            Properties.Add(new MachineProperty("Angle Stability", _propertyAngleStability));
            Properties.Add(new MachineProperty("Angular Velocity Stability", _propertyAngularVelocityStability));


            _initRotation = transform.rotation.eulerAngles.z;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (SimulatorRigidBody)
            {
                // 根据角度计算力矩,力矩只与角度符号有关
                float angle = _initRotation - transform.rotation.eulerAngles.z;
                angle = (angle - 360 * Mathf.FloorToInt((angle + 180f) / 360f)) > 0 ? 1 : -1;

                // 根据角速度修正力矩,防止出现抖动
                float angularVelocity =
                    transform.InverseTransformDirection(SimulatorRigidBody.angularVelocity).z > 0
                        ? 1
                        : -1;

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
                // Debug.DrawLine(transform.position, transform.position + transform.forward * 10, Color.red);
                // Debug.DrawLine(transform.position, transform.position + transform.forward * torque, Color.blue);
            }
        }
    }
}