using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TheLastTour.Controller.Machine
{
    public class SpringPart : MovablePart
    {
        public AudioSource audioSource;
        public AudioClip audioClip;
        
        
        public LineRenderer springRenderer;

        public List<Vector2> springPoints = new List<Vector2>()
        {
            new Vector2(0.3f, 0),
            new Vector2(0, 0.3f),
            new Vector2(-0.3f, 0),
            new Vector2(0, -0.3f),
        };

        // Spring base 连接在车身上
        // Spring top 作为弹簧的连接点
        public GameObject springBase;
        public GameObject springTop;

        // SpringJoint 连接 SpringBase 和 SpringTop
        public SpringJoint springJoint;
        private Vector3 _springDirection = Vector3.right;

        private readonly PropertyValue<Key> _propertyTrigger = new PropertyValue<Key>(Key.None);
        private readonly PropertyValue<float> _propertySpringStrength = new PropertyValue<float>(3000);
        private readonly PropertyValue<float> _propertySpringLength = new PropertyValue<float>(0.3f);
        private readonly PropertyValue<float> _propertySpringMultiplier = new PropertyValue<float>(2f);

        private void UpdateSpring(float springLength, bool updateSpringTop = false)
        {
            // Spring Top Anchor 位置
            springJoint.anchor =
                -_springDirection * springLength / 2 / springTop.transform.localScale.x;
            if (updateSpringTop)
            {
                springTop.transform.localPosition = _springDirection * springLength;
            }

            if (springJoint.connectedBody == null)
            {
                return;
            }

            // 本体 Anchor 位置
            // 局部坐标 => 世界坐标 => 本体局部坐标
            Vector3 position =
                (_springDirection * springLength / 2) +
                springBase.transform.localPosition;
            position = transform.TransformPoint(position);
            springJoint.connectedAnchor = springJoint.connectedBody.transform.InverseTransformPoint(position);
        }

        public override void OnAttached(ISimulator simulator)
        {
            base.OnAttached(simulator);
            springJoint.connectedBody = simulator.GetSimulatorRigidbody();
            springJoint.autoConfigureConnectedAnchor = false;
            springJoint.enableCollision = true;

            // 取中点作为平衡点
            springTop.transform.position = _springDirection * _propertySpringLength.Value;
            UpdateSpring(_propertySpringLength.Value, true);
        }


        protected override void InitProperties()
        {
            base.InitProperties();

            Properties.Add(new MachineProperty("Trigger", _propertyTrigger));
            Properties.Add(new MachineProperty("Spring Strength", _propertySpringStrength));
            Properties.Add(new MachineProperty("Spring Length", _propertySpringLength));
            Properties.Add(new MachineProperty("Spring Multiplier", _propertySpringMultiplier));

            springJoint.spring = _propertySpringStrength.Value;
            springJoint.minDistance = 0;
            springJoint.maxDistance = 0;

            _propertySpringLength.OnValueChanged += (f) => { UpdateSpring(f, true); };
            UpdateSpring(_propertySpringLength.Value, true);

            SimulatorRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            
            audioSource.clip = audioClip;
            audioSource.loop = false;
        }

        public override void AddPart(PartController part)
        {
            // 与普通的 MovablePart 不同, 以 SpringTop 作为连接点
            attachedParts.Add(part);
            part.transform.parent = springTop.transform;
            part.OnAttached(this);
            UpdateSimulatorMass();
        }

        public override void Update()
        {
            if (_propertyTrigger.Value != Key.None)
            {
                if (Keyboard.current[_propertyTrigger.Value].wasPressedThisFrame)
                {
                    audioSource.Play();
                    UpdateSpring(_propertySpringLength.Value * _propertySpringMultiplier.Value);
                }
                else if (Keyboard.current[_propertyTrigger.Value].wasReleasedThisFrame)
                {
                    UpdateSpring(_propertySpringLength.Value);
                }
            }
        }


        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (SimulatorRigidbody && SimulatorRigidbody.isKinematic == false)
            {
                Vector3 localVelocity = transform.InverseTransformDirection(SimulatorRigidbody.velocity);
                localVelocity.y = 0;
                localVelocity.z = 0;
                SimulatorRigidbody.velocity = transform.TransformDirection(localVelocity);

                springTop.transform.localPosition = new Vector3(
                    springTop.transform.localPosition.x,
                    0, 0
                );

                // MovablePartRigidbody.angularVelocity = Vector3.zero;
                springTop.transform.localRotation = Quaternion.identity;
            }

            if (springRenderer)
            {
                float sprintLength = springTop.transform.localPosition.x;
                for (int i = 0; i < springRenderer.positionCount; i++)
                {
                    int index = i % springPoints.Count;
                    Vector3 position = new Vector3(
                        i * sprintLength / (springRenderer.positionCount - 1),
                        springPoints[index].x,
                        springPoints[index].y
                    );

                    position = springBase.transform.rotation * position + springBase.transform.position;
                    springRenderer.SetPosition(i, position);
                }
            }
        }
    }
}