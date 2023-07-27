using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheLastTour.Controller.Machine
{
    /// <summary>
    /// MovablePart 是可以移动的 Part,需要添加 Rigidbody 组件
    /// 包括可以主动运动的轮子,传动轴轴,也包括只能被动运动的关节
    /// 其运动由自身的 Rigidbody 组件模拟,同时通过 Joint 与父级 Rigidbody 连接
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class MovablePart : PartController
    {
        private Rigidbody _rigidbody;
        
        public List<PartController> attachedParts = new List<PartController>();

        public Rigidbody MovablePartRigidbody
        {
            get
            {
                if (_rigidbody == null)
                {
                    _rigidbody = GetComponent<Rigidbody>();
                }

                return _rigidbody;
            }
        }


        public override Rigidbody GetSimulatorRigidbody()
        {
            return MovablePartRigidbody;
        }

        public override void AddPart(PartController part)
        {
            throw new System.NotImplementedException();
        }

        public override void RemovePart(PartController part)
        {
            throw new System.NotImplementedException();
        }

        public override void UpdateMachineMass()
        {
            throw new System.NotImplementedException();
        }
    }
}