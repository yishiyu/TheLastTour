using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TheLastTour.Controller.Machine
{
    public class HingePart : MovablePart
    {
        public GameObject hingeBase;
        public GameObject hingeTop;

        public HingeJoint hingePartJoint;

        public override void OnAttached(ISimulator simulator)
        {
            base.OnAttached(simulator);
            hingePartJoint.connectedBody = simulator.GetSimulatorRigidbody();
            hingePartJoint.enableCollision = true;
        }

        public override void AddPart(PartController part)
        {
            // 与普通的 MovablePart 不同, 以 SpringTop 作为连接点
            attachedParts.Add(part);
            part.transform.parent = hingeTop.transform;
            part.OnAttached(this);
            UpdateSimulatorMass();
        }
    }
}