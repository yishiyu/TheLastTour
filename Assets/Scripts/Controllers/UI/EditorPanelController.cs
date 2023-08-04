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

        public Button prefabSelectButton;
        public GameObject prefabButtonGroup;

        private IGameStateManager _gameStateManager;
        private GameManager _gameManager;


        private void Start()
        {
            _gameStateManager = TheLastTourArchitecture.Instance.GetManager<IGameStateManager>();
            _gameManager = GameManager.Instance;

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
            }
        }
    }
}