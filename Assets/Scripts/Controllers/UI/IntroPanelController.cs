using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace TheLastTour.Controller.UI
{
    public class IntroPanelController : MonoBehaviour
    {
        public List<CanvasGroup> canvasGroups;

        IEnumerator ShowIntroduction()
        {
            foreach (var canvasGroup in canvasGroups)
            {
                yield return new WaitForSeconds(0.5f);
                canvasGroup.DOFade(1, 1);

                yield return new WaitForSeconds(3);

                canvasGroup.DOFade(0, 1);
            }

            yield return new WaitForSeconds(1f);
            gameObject.SetActive(false);
        }


        void Start()
        {
            foreach (var canvasGroup in canvasGroups)
            {
                canvasGroup.alpha = 0;
            }

            StartCoroutine(ShowIntroduction());
        }


        private void Update()
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                StopAllCoroutines();
                gameObject.SetActive(false);
            }
        }
    }
}