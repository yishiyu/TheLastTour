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
            _gameManager = GameManager.Instance;
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
                debugInfoText.text = "~: clear prefab part select\n" +
                                     "tab: delete the select part\n" +
                                     "GameMode: " + _gameStateManager.GameState + "\n" +
                                     "EditMode: " + _gameStateManager.EditState + "\n" +
                                     "SelectedPartIndex: " + _gameManager.CurrentSelectedPartIndex + "\n";

                if (_gameManager.selectedPart != null)
                {
                    debugInfoText.text += "SelectedPart: " + _gameManager.selectedPart + "\n";
                }
                else
                {
                    debugInfoText.text += "SelectedPart: " + "\n";
                }
            }
        }
    }
}