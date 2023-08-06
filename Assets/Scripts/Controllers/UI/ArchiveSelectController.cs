using System;
using System.Collections;
using System.Collections.Generic;
using TheLastTour.Manager;
using TheLastTour.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastTour.Controller.UI
{
    public class ArchiveSelectController : MonoBehaviour
    {
        public Text archiveName;
        public Button loadButton;
        public Button deleteButton;

        private void Awake()
        {
            if (loadButton)
            {
                loadButton.onClick.AddListener(
                    (() =>
                    {
                        Debug.Log("Load " + archiveName.text);
                        TheLastTourArchitecture.Instance.GetManager<IMachineManager>()?.LoadMachines(archiveName.text);
                    })
                );
            }

            if (deleteButton)
            {
                deleteButton.onClick.AddListener(
                    (() =>
                    {
                        Debug.Log("Delete " + archiveName.text);
                        TheLastTourArchitecture.Instance.GetUtility<IArchiveUtility>()
                            ?.DeleteArchive("Machines", archiveName.text);
                        Destroy(gameObject);
                    })
                );
            }
        }
    }
}