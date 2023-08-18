using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheLastTour.Controller.Machine
{
    public class BalloonPart : MovablePart
    {
        public LineRenderer lineRenderer;
        public ConfigurableJoint balloonJoint;

        public Transform basePosition;
        public Transform topPosition;


        private readonly PropertyValue<float> _propertyPower = new PropertyValue<float>(1f);


        public override void TurnOnSimulation(bool isOn)
        {
            base.TurnOnSimulation(isOn);

            SimulatorRigidbody.mass = 1f;
            SimulatorRigidbody.useGravity = false;
        }

        protected override void InitProperties()
        {
            base.InitProperties();
            
            Properties.Add(new MachineProperty("Power", _propertyPower));
        }


        public override void OnAttached(ISimulator simulator)
        {
            base.OnAttached(simulator);

            balloonJoint.connectedBody = simulator.GetSimulatorRigidbody();

            balloonJoint.autoConfigureConnectedAnchor = false;
            balloonJoint.connectedAnchor =
                balloonJoint.connectedBody.transform.InverseTransformPoint(basePosition.position);
        }

        public override void Update()
        {
            // 气球放飞有点小问题, 先不要这个功能了
            // base.Update();

            lineRenderer.SetPosition(0, basePosition.position);
            lineRenderer.SetPosition(1, topPosition.position);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (SimulatorRigidbody)
            {
                SimulatorRigidbody.AddForce(
                    Vector3.up * _propertyPower.Value,
                    ForceMode.Impulse);
            }
        }
    }
}