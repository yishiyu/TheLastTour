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
        public GameObject debugInfoPanel;
        public Text debugInfoText;

        public GameObject editorPanel;

        public GameObject compassPanel;
        
        public GameObject pausePanel;

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
                    compassPanel.SetActive(false);
                    editorPanel.SetActive(true);
                    debugInfoPanel.SetActive(true);
                    pausePanel.SetActive(false);
                    break;
                case EGameState.Play:
                    compassPanel.SetActive(true);
                    editorPanel.SetActive(false);
                    debugInfoPanel.SetActive(false);
                    pausePanel.SetActive(false);
                    break;
                case EGameState.Pause:
                    compassPanel.SetActive(false);
                    editorPanel.SetActive(false);
                    debugInfoPanel.SetActive(false);
                    pausePanel.SetActive(true);
                    break;
                case EGameState.GameOver:
                    compassPanel.SetActive(false);
                    editorPanel.SetActive(false);
                    debugInfoPanel.SetActive(false);
                    pausePanel.SetActive(false);
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