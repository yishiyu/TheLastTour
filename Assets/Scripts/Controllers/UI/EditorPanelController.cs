using System;
using System.Collections;
using System.Collections.Generic;
using TheLastTour.Event;
using TheLastTour.Manager;
using TheLastTour.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastTour.Controller.UI
{
    public class EditorPanelController : MonoBehaviour
    {
        public Button playButton;

        public InputField saveInputField;
        public Button saveButton;

        public Button loadButton;
        public Button closeLoadPanelButton;
        public GameObject loadPanel;
        public Transform archiveContainer;
        public ArchiveSelectController archiveSelectPrefab;

        public Button prefabSelectButton;
        public GameObject prefabButtonGroup;

        private IGameStateManager _gameStateManager;
        private GameManager _gameManager;
        private IMachineManager _machineManager;
        private IArchiveUtility _archiveUtility;

        private void Start()
        {
            _gameManager = GameManager.Instance;
            _gameStateManager = TheLastTourArchitecture.Instance.GetManager<IGameStateManager>();
            _machineManager = TheLastTourArchitecture.Instance.GetManager<IMachineManager>();
            _archiveUtility = TheLastTourArchitecture.Instance.GetUtility<IArchiveUtility>();

            playButton.onClick.AddListener(
                (() => { _gameStateManager.GameState = EGameState.Play; })
            );

            saveButton.onClick.AddListener(
                (() =>
                {
                    if (saveInputField)
                    {
                        string saveName = saveInputField.text;
                        if (saveName.Length > 0)
                        {
                            _machineManager?.SaveMachines(saveName);
                        }
                    }
                })
            );

            loadButton.onClick.AddListener(
                (() =>
                {
                    for (int i = 0; i < archiveContainer.childCount; i++)
                    {
                        Destroy(archiveContainer.GetChild(i).gameObject);
                    }

                    List<string> archives = _archiveUtility.SearchArchive("Machines");
                    foreach (var archive in archives)
                    {
                        Debug.Log(archive);
                        var archiveSelect = Instantiate(archiveSelectPrefab, archiveContainer);
                        archiveSelect.archiveName.text = archive;
                    }

                    loadPanel.SetActive(true);
                })
            );

            closeLoadPanelButton.onClick.AddListener(
                (() => { loadPanel.SetActive(false); })
            );

            for (int i = 0; i < _gameManager.partPrefabs.Count; i++)
            {
                var button = Instantiate(prefabSelectButton, prefabButtonGroup.transform);
                button.GetComponentInChildren<Text>().text = _gameManager.partPrefabs[i].partName;

                int index = i;
                button.onClick.AddListener(
                    (() => { _gameManager.CurrentSelectedPartIndex = index; })
                );
            }
        }
    }
}