using System;
using System.Collections;
using System.Collections.Generic;
using TheLastTour.Event;
using TheLastTour.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastTour.Controller.UI
{
    public class EditorPanelController : MonoBehaviour
    {
        public Button playButton;
        public Button saveButton;
        public Button loadButton;

        public Button prefabSelectButton;
        public GameObject prefabButtonGroup;

        private IGameStateManager _gameStateManager;
        private GameManager _gameManager;
        private IMachineManager _machineManager;

        private void Start()
        {
            _gameManager = GameManager.Instance;
            _gameStateManager = TheLastTourArchitecture.Instance.GetManager<IGameStateManager>();
            _machineManager = TheLastTourArchitecture.Instance.GetManager<IMachineManager>();

            playButton.onClick.AddListener(
                (() => { _gameStateManager.GameState = EGameState.Play; })
            );


            for (int i = 0; i < _gameManager.partPrefabs.Count; i++)
            {
                var button = Instantiate(prefabSelectButton, prefabButtonGroup.transform);
                button.GetComponentInChildren<Text>().text = _gameManager.partPrefabs[i].partName;

                int index = i;
                button.onClick.AddListener(
                    (() => { _gameManager.CurrentSelectedPartIndex = index; })
                );

                saveButton.onClick.AddListener(
                    (() => { _machineManager?.SaveMachines("saved machines"); })
                );

                loadButton.onClick.AddListener(
                    (() =>
                    {
                        if (_machineManager != null)
                        {
                            _machineManager.LoadMachines("saved machines");
                            var corePart = _machineManager.GetCorePart();
                            if (corePart)
                            {
                                GameEvents.FocusOnTargetEvent.Target = _machineManager.GetCorePart().transform;
                                EventBus.Invoke(GameEvents.FocusOnTargetEvent);
                            }
                        }
                    })
                );
            }
        }
    }
}