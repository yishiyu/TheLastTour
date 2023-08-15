using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TheLastTour.Controller.Machine
{
    public class BasketPart : FixedPart
    {
        public PartJointController detachJoint;


        private readonly PropertyValue<Key> _propertyRelease = new PropertyValue<Key>(Key.None);

        protected override void InitProperties()
        {
            base.InitProperties();

            Properties.Add(new MachineProperty("Release", _propertyRelease));
        }

        public override void Update()
        {
            if (_propertyRelease.Value != Key.None)
            {
                if (Keyboard.current[_propertyRelease.Value].wasPressedThisFrame)
                {
                    // 释放载荷并开启其物理模拟
                    DetachJoint(detachJoint, false)?.TurnOnSimulation(true);
                }
            }
        }
    }
}