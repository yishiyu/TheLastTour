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
        public GameObject editorPanel;

        private GameManager _gameManager;
        private IGameStateManager _gameStateManager;

        private void Start()
        {
            _gameManager = GameManager.Instance;
            _gameStateManager = TheLastTourArchitecture.Instance.GetManager<IGameStateManager>();
        }

        #region Event Handlers

        private void OnEnable()
        {
            EventBus.AddListener<GameStateChangedEvent>(OnGameStateChanged);
        }

        private void OnDisable()
        {
            EventBus.RemoveListener<GameStateChangedEvent>(OnGameStateChanged);
        }

        private void OnGameStateChanged(GameStateChangedEvent evt)
        {
            switch (evt.CurrentState)
            {
                case EGameState.Edit:
                    editorPanel.SetActive(true);
                    break;
                case EGameState.Play:
                    editorPanel.SetActive(false);
                    break;
                case EGameState.Pause:
                    editorPanel.SetActive(false);
                    break;
                case EGameState.GameOver:
                    editorPanel.SetActive(false);
                    break;
            }
        }

        #endregion

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