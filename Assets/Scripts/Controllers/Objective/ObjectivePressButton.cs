using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheLastTour.Controller.Objective
{
    public abstract class ObjectivePressButtonInteractableObject : MonoBehaviour
    {
        public abstract void OnPressButton();

        public abstract void OnReleaseButton();
    }

    [RequireComponent(typeof(Collider))]
    public class ObjectivePressButton : Manager.Objective
    {
        public List<ObjectivePressButtonInteractableObject> interactObjects =
            new List<ObjectivePressButtonInteractableObject>();

        private bool isPressed = false;

        public GameObject buttonMesh;
        public Transform buttonMeshPressedTransform;
        public Transform buttonMeshReleasedTransform;
        public float buttonMeshPressedTime = 1f;


        public override void Start()
        {
            base.Start();
            buttonMesh.transform.position = buttonMeshReleasedTransform.position;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isPressed)
            {
                isPressed = true;
                foreach (var interactObject in interactObjects)
                {
                    interactObject.OnPressButton();
                }

                StopAllCoroutines();
                StartCoroutine(UpdateButtonMesh());
            }
        }


        private void OnTriggerExit(Collider other)
        {
            if (isPressed)
            {
                isPressed = false;
                foreach (var interactObject in interactObjects)
                {
                    interactObject.OnReleaseButton();
                }

                StopAllCoroutines();
                StartCoroutine(UpdateButtonMesh());
            }
        }

        IEnumerator UpdateButtonMesh()
        {
            Vector3 targetPosition = buttonMeshReleasedTransform.localPosition;

            if (isPressed)
            {
                targetPosition = buttonMeshPressedTransform.localPosition;
            }
            else
            {
                targetPosition = buttonMeshReleasedTransform.localPosition;
            }

            while ((Vector3.Distance(buttonMesh.transform.localPosition, targetPosition)) > 0.01f)
            {
                buttonMesh.transform.localPosition =
                    Vector3.Lerp(buttonMesh.transform.localPosition,
                        targetPosition, 0.1f / buttonMeshPressedTime);
                Debug.Log("Button Mesh Position: " + buttonMesh.transform.localPosition);
                yield return null;
            }
        }
    }
}