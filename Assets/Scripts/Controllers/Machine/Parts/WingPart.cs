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
        Rigidbody _simulatorRigidBody;

        Rigidbody SimulatorRigidBody
        {
            get
            {
                if (_simulatorRigidBody == null)
                {
                    _simulatorRigidBody = GetComponentInParent<Rigidbody>();
                }

                return _simulatorRigidBody;
            }
        }


        readonly PropertyValue<float> _propertyLiftRatio = new PropertyValue<float>(1);


        protected override void InitProperties()
        {
            base.InitProperties();


            Properties.Add(new MachineProperty("Lift Ratio", _propertyLiftRatio));
        }


        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (SimulatorRigidBody)
            {
                // 局部速度(局部坐标)
                float speed = Vector3.Dot(transform.forward,
                    SimulatorRigidBody.GetPointVelocity(transform.position));
                float lightForce = speed * speed * _propertyLiftRatio.Value / 100;

                SimulatorRigidBody.AddForceAtPosition(
                    lightForce * transform.up,
                    transform.position,
                    ForceMode.Impulse);
                // RigidBody.AddRelativeForce(
                //     lightForce * transform.up,
                //     transform.localPosition,
                //     ForceMode.Impulse
                // );


                Debug.DrawLine(transform.position, transform.position + transform.forward * 10, Color.red);
                Debug.DrawLine(transform.position, transform.position + transform.up * lightForce, Color.blue);
            }
        }
    }
}