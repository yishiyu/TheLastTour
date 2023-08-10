using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using TheLastTour.Manager;
using Unity.Properties;
using UnityEngine;
using UnityEngine.InputSystem;
using Quaternion = System.Numerics.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace TheLastTour.Controller.Machine
{
    public class WingPart : FixedPart
    {
        readonly PropertyValue<float> _propertyLiftRatio = new PropertyValue<float>(1);


        protected override void InitProperties()
        {
            base.InitProperties();


            Properties.Add(new MachineProperty("Lift Ratio", _propertyLiftRatio));
        }


        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (SimulatorRigidbody)
            {
                // 局部速度(局部坐标)
                float speed = Vector3.Dot(SimulatorRigidbody.GetPointVelocity(transform.position), transform.forward);
                float lightForce = speed * _propertyLiftRatio.Value / 100;

                Vector3 force = lightForce * transform.up;
                Vector3 torque = Vector3.Cross(transform.up, force);

                SimulatorRigidbody.AddForce(
                    force,
                    ForceMode.Impulse);

                // SimulatorRigidbody.AddTorque(
                //     torque,
                //     ForceMode.Impulse);

                // RigidBody.AddRelativeForce(
                //     lightForce * transform.up,
                //     transform.localPosition,
                //     ForceMode.Impulse
                // );

                if (IsDrawGizmos)
                {
                    // Debug.DrawLine(transform.position, transform.position + transform.forward * 10, Color.red);
                    Debug.DrawLine(transform.position, transform.position + transform.up * lightForce, Color.blue);
                }
            }
        }
    }
}