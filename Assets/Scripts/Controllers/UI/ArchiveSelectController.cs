using System;
using System.Collections;
using System.Collections.Generic;
using TheLastTour.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastTour.Controller.UI
{
    [RequireComponent(typeof(Button))]
    public class ArchiveSelectController : MonoBehaviour
    {
        public Text archiveName;
        public Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(
                (() =>
                {
                    Debug.Log("Clicked " + archiveName.text);
                    TheLastTourArchitecture.Instance.GetManager<IMachineManager>()?.LoadMachines(archiveName.text);
                })
            );
        }
    }
}