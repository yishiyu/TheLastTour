using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TheLastTour.Controller.Machine
{
    /// <summary>
    /// FixedPart 是固定 Part,不能在机器内部相对运动
    /// 其物理模拟托管到父级(包括 Machine 和父级 MovablePart)的 Rigidbody 上
    /// </summary>
    public class FixedPart : PartController
    {
        public override Rigidbody GetSimulatorRigidbody()
        {
            if (transform.parent)
            {
                // 将自身的物理模拟托管到父级,无论是 Machine 还是 MovablePart
                return transform.parent.GetComponentInParent<ISimulator>().GetSimulatorRigidbody();
            }

            return null;
        }

        public override void AddPart(PartController part)
        {
            transform.parent.GetComponentInParent<ISimulator>().AddPart(part);
        }

        public override void RemovePart(PartController part, bool destroyPart)
        {
            transform.parent.GetComponentInParent<ISimulator>().RemovePart(part, destroyPart);
        }

        public override void UpdateSimulatorMass()
        {
            if (transform.parent)
            {
                transform.parent.GetComponentInParent<ISimulator>()?.UpdateSimulatorMass();
            }
        }

        public override ISimulator DetachJoint(PartJointController joint, bool simulate)
        {
            if (transform.parent)
            {
                ISimulator simulator = transform.parent.GetComponentInParent<ISimulator>();
                if (simulator != null)
                {
                    return simulator.DetachJoint(joint, simulate);
                }
            }

            return null;
        }

        public override void Update()
        {
            base.Update();
        }
    }
}