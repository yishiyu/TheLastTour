using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheLastTour.Controller.Machine
{
    public class GyroPart : FixedPart
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

        readonly PropertyValue<float> _propertyStability = new PropertyValue<float>(0.5f);

        protected override void InitProperties()
        {
            base.InitProperties();

            Properties.Add(new MachineProperty("Stability", _propertyStability));
            _propertyStability.OnValueChanged += (f) =>
            {
                if (f < 0 || f > 1)
                {
                    _propertyStability.Value = Mathf.Clamp(_propertyStability.Value, 0.01f, 0.99f);
                }
            };
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (RigidBody && !RigidBody.isKinematic)
            {
                // 计算角速度
                Vector3 angularVelocity = RigidBody.angularVelocity;
                Debug.Log("angularVelocity: " + angularVelocity);

                angularVelocity *= 1 - _propertyStability.Value * Time.fixedDeltaTime;
                // 局部速度(局部坐标)
                RigidBody.angularVelocity = angularVelocity;
            }
        }
    }
}