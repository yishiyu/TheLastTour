using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TheLastTour.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastTour.Controller.UI
{
    public class TheLastTourUIController : MonoBehaviour
    {
        public Text debugInfoText;

        private GameManager _gameManager;
        private IGameStateManager _gameStateManager;

        private void Start()
        {
            _gameManager = FindObjectOfType<GameManager>().GetComponent<GameManager>();
            _gameStateManager = TheLastTourArchitecture.Instance.GetManager<IGameStateManager>();
        }

        private void Update()
        {
            UpdateDebugInfo();
        }

        private void UpdateDebugInfo()
        {
            if (debugInfoText)
            {
                debugInfoText.text = "1: place mode\n" +
                                     "2: select mode\n" +
                                     "3: select empty part prefab\n" +
                                     "4: select cube part prefab\n" +
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