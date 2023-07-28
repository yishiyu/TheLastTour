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


        readonly PropertyValue<float> _propertyLiftRatio = new PropertyValue<float>(1);


        protected override void InitProperties()
        {
            base.InitProperties();


            Properties.Add(new MachineProperty("Lift Ratio", _propertyLiftRatio));
        }


        private void FixedUpdate()
        {
            if (RigidBody)
            {
                // 局部速度(局部坐标)
                Vector3 velocity = transform.rotation * RigidBody.GetPointVelocity(transform.position);
                

                RigidBody.AddForceAtPosition(
                    velocity.z * _propertyLiftRatio.Value * transform.up,
                    transform.position,
                    ForceMode.Impulse);

                Debug.Log("velocity.z: " + velocity.z +
                          " _propertyLiftRatio.Value: " + _propertyLiftRatio.Value +
                          " lift force: " + (velocity.z * _propertyLiftRatio.Value) +
                          "velocity " + velocity);

                // draw debug line
                Debug.DrawLine(transform.position,
                    transform.position + transform.up * 10,
                    Color.red);
                Debug.DrawLine(transform.position,
                    transform.position + velocity.z * transform.forward,
                    Color.blue);
            }
        }
    }
}