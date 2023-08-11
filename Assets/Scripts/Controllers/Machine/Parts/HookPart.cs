using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TheLastTour.Controller.Machine
{
    public class HookPart : MovablePart
    {
        public LineRenderer springRenderer;
        public GameObject hookBase;
        public GameObject hookTop;

        public ConfigurableJoint hookJoint;

        public Hook hook;

        private readonly PropertyValue<Key> _propertyTrigger = new PropertyValue<Key>(Key.None);
        private readonly PropertyValue<Key> _propertyRelease = new PropertyValue<Key>(Key.None);
        private readonly PropertyValue<float> _propertyHookLength = new PropertyValue<float>(10f);
        private readonly PropertyValue<float> _propertyHookStrength = new PropertyValue<float>(1000f);

        enum HookState
        {
            Ready,
            Launched,
            Retracting,
        }

        private HookState _hookState = HookState.Ready;
        private float _hookMinLength = 0f;

        public void LaunchHook()
        {
            if (_hookState == HookState.Ready)
            {
                _hookState = HookState.Launched;
                hook.Release();
                hookJoint.linearLimit = new SoftJointLimit()
                {
                    limit = _propertyHookLength.Value,
                };

                SimulatorRigidbody.AddForce(
                    hookBase.transform.right * _propertyHookStrength.Value,
                    ForceMode.Force
                );
                
                hookJoint.angularXMotion = ConfigurableJointMotion.Free;
                hookJoint.angularYMotion = ConfigurableJointMotion.Free;
                hookJoint.angularZMotion = ConfigurableJointMotion.Free;
            }
        }

        public void RetractHook()
        {
            if (_hookState == HookState.Launched)
            {
                _hookState = HookState.Retracting;
                StartCoroutine(DoRetract());
            }
        }

        public IEnumerator DoRetract()
        {
            _hookState = HookState.Retracting;

            // Do Retraction
            float distance = Vector3.Distance(hookBase.transform.position, hookTop.transform.position);
            float decayRate = 0.9f;
            while (distance > (_hookMinLength + 0.3f))
            {
                hookJoint.linearLimit = new SoftJointLimit()
                {
                    limit = distance,
                };

                distance *= (1 - decayRate * Time.deltaTime);
                // distance = Mathf.SmoothDamp(distance, _hookMinLength, ref velocity, 1f);
                Debug.Log("distance: " + distance);
                yield return null;
            }

            hookJoint.linearLimit = new SoftJointLimit()
            {
                limit = _hookMinLength,
            };

            _hookState = HookState.Ready;
            
            hookJoint.angularXMotion = ConfigurableJointMotion.Locked;
            hookJoint.angularYMotion = ConfigurableJointMotion.Locked;
            hookJoint.angularZMotion = ConfigurableJointMotion.Locked;
            yield return null;
        }

        public void Release()
        {
            hook.Release();
        }

        public override void OnAttached(ISimulator simulator)
        {
            base.OnAttached(simulator);

            hookJoint.connectedBody = simulator.GetSimulatorRigidbody();
            hookJoint.enableCollision = true;

            Vector3 position =
                hookJoint.connectedBody.transform.InverseTransformPoint(hookBase.transform.position +
                                                                        hookBase.transform.right * 0.3f);
            hookJoint.autoConfigureConnectedAnchor = false;
            hookJoint.connectedAnchor = position;
        }


        protected override void InitProperties()
        {
            base.InitProperties();

            Properties.Add(new MachineProperty("Trigger", _propertyTrigger));
            Properties.Add(new MachineProperty("Release", _propertyRelease));
            Properties.Add(new MachineProperty("Hook Length", _propertyHookLength));

            hookJoint.anchor = Vector3.zero;
            hookJoint.xMotion = ConfigurableJointMotion.Limited;
            hookJoint.yMotion = ConfigurableJointMotion.Limited;
            hookJoint.zMotion = ConfigurableJointMotion.Limited;
            hookJoint.angularXMotion = ConfigurableJointMotion.Locked;
            hookJoint.angularYMotion = ConfigurableJointMotion.Locked;
            hookJoint.angularZMotion = ConfigurableJointMotion.Locked;
            hookJoint.linearLimit = new SoftJointLimit()
            {
                limit = _hookMinLength,
            };
        }



        private void Update()
        {
            if (_propertyTrigger.Value != Key.None)
            {
                if (Keyboard.current[_propertyTrigger.Value].wasPressedThisFrame)
                {
                    // 发射钩子
                    if (_hookState == HookState.Ready)
                    {
                        LaunchHook();
                    }
                    else if (_hookState == HookState.Launched)
                    {
                        RetractHook();
                    }
                }
            }

            if (_propertyRelease.Value != Key.None)
            {
                if (Keyboard.current[_propertyRelease.Value].wasPressedThisFrame)
                {
                    // 松开钩子
                    Release();
                }
            }

            springRenderer.SetPosition(0, hookBase.transform.position);
            springRenderer.SetPosition(1, hookTop.transform.position);
        }
    }
}