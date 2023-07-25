using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TheLastTour.Event;
using TheLastTour.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastTour.Controller.UI
{
    public class TheLastTourUIController : MonoBehaviour
    {
        public Text debugInfoText;
        public GameObject EditorPanel;

        private GameManager _gameManager;
        private IGameStateManager _gameStateManager;

        private void Start()
        {
            _gameManager = FindObjectOfType<GameManager>().GetComponent<GameManager>();
            _gameStateManager = TheLastTourArchitecture.Instance.GetManager<IGameStateManager>();
        }

        private void OnEnable()
        {
            EventBus.AddListener<GameStateChangedEvent>(OnGameStateChanged);
        }

        private void OnGameStateChanged(GameStateChangedEvent evt)
        {
            switch (evt.CurrentState)
            {
                case EGameState.Edit:
                    EditorPanel.SetActive(true);
                    break;
                case EGameState.Play:
                    EditorPanel.SetActive(false);
                    break;
                case EGameState.Pause:
                    EditorPanel.SetActive(false);
                    break;
                case EGameState.GameOver:
                    EditorPanel.SetActive(false);
                    break;
            }
        }

        private void Update()
        {
            UpdateDebugInfo();
        }

        private void UpdateDebugInfo()
        {
            if (debugInfoText)
            {
                debugInfoText.text = "7: select empty part prefab\n" +
                                     "8: select cube part prefab\n" +
                                     "GameMode: " + _gameStateManager.GameState + "\n" +
                                     "EditMode: " + _gameStateManager.EditState + "\n" +
                                     "SelectedPartIndex: " + _gameManager.CurrentSelectedPartIndex + "\n" +
                                     "SelectedParts: " + _gameManager.selectedParts.Count + "\n";

                foreach (var part in _gameManager.selectedParts)
                {
                    debugInfoText.text += part.name + "\n";
                }
            }
        }
    }
}