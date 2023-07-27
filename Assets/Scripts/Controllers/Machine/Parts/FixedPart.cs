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
            // 将自身的物理模拟托管到父级,无论是 Machine 还是 MovablePart
            return GetComponentInParent<ISimulator>().GetSimulatorRigidbody();
        }

        public override void AddPart(PartController part)
        {
            throw new NotImplementedException();
        }

        public override void RemovePart(PartController part)
        {
            throw new NotImplementedException();
        }

        public override void UpdateMachineMass()
        {
            throw new NotImplementedException();
        }
    }
}