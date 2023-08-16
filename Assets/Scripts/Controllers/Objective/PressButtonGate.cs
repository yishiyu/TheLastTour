using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheLastTour.Controller.Objective
{
    public class PressButtonGate : ObjectivePressButtonInteractableObject
    {
        public GameObject gateMesh;
        public Transform gateMeshOpenTransform;
        public Transform gateMeshClosedTransform;

        public float gateMeshOpenTime = 1f;
        public bool isOpen = false;

        IEnumerator UpdateGate()
        {
            Vector3 gatePosition = gateMesh.transform.localPosition;
            Quaternion gateRotation = gateMesh.transform.localRotation;

            if (isOpen)
            {
                gatePosition = gateMeshOpenTransform.localPosition;
                gateRotation = gateMeshOpenTransform.localRotation;
            }
            else
            {
                gatePosition = gateMeshClosedTransform.localPosition;
                gateRotation = gateMeshClosedTransform.localRotation;
            }

            while (Vector3.Distance(gateMesh.transform.localPosition, gatePosition) > 0.01f ||
                   Quaternion.Angle(gateMesh.transform.localRotation, gateRotation) > 0.01f)
            {
                gateMesh.transform.localPosition = Vector3.Lerp(gateMesh.transform.localPosition, gatePosition,
                    Time.deltaTime / gateMeshOpenTime);
                gateMesh.transform.localRotation = Quaternion.Lerp(gateMesh.transform.localRotation, gateRotation,
                    Time.deltaTime / gateMeshOpenTime);
                yield return null;
            }
        }

        public override void OnPressButton()
        {
            isOpen = true;
            StopAllCoroutines();
            StartCoroutine(UpdateGate());
        }

        public override void OnReleaseButton()
        {
            isOpen = false;
            StopAllCoroutines();
            StartCoroutine(UpdateGate());
        }
    }
}