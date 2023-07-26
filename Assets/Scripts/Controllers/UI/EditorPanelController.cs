using System;
using System.Collections;
using System.Collections.Generic;
using TheLastTour.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastTour.Controller.UI
{
    public class EditorPanelController : MonoBehaviour
    {
        public Button PlayButton;

        public Button PrefabSelectButton;
        public GameObject PrefabButtonGroup;

        private IGameStateManager _gameStateManager;
        private GameManager _gameManager;


        private void Start()
        {
            _gameStateManager = TheLastTourArchitecture.Instance.GetManager<IGameStateManager>();
            _gameManager = GameManager.Instance;

            PlayButton.onClick.AddListener(
                (() => { _gameStateManager.GameState = EGameState.Play; })
            );


            for (int i = 0; i < _gameManager.partPrefabs.Count; i++)
            {
                var button = Instantiate(PrefabSelectButton, PrefabButtonGroup.transform);
                button.GetComponentInChildren<Text>().text = _gameManager.partPrefabs[i].name;

                int index = i;
                button.onClick.AddListener(
                    (() => { _gameManager.CurrentSelectedPartIndex = index; })
                );
            }
        }
    }
}