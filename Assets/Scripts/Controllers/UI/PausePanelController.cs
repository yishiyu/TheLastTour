using System;
using System.Collections;
using System.Collections.Generic;
using TheLastTour.Manager;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TheLastTour.Controller.UI
{
    public class PausePanelController : MonoBehaviour
    {
        public GameObject controlPanel;
        public Button controlPanelButton;

        public GameObject levelsPanel;
        public Button levelsPanelButton;

        public Button closeButton;

        // Settings
        public Toggle showDebugToggle;

        public Button restartButton;
        public Button exitButton;

        // 音量设置
        public Slider masterVolumeSlider;

        private IGameStateManager _gameStateManager;

        private void Start()
        {
            _gameStateManager = TheLastTourArchitecture.Instance.GetManager<IGameStateManager>();

            closeButton.onClick.AddListener(
                () => { _gameStateManager.ChaneToPreviousState(); }
            );

            showDebugToggle.isOn = _gameStateManager.DebugMode;
            showDebugToggle.onValueChanged.AddListener(
                (value) => { TheLastTourArchitecture.Instance.GetManager<IGameStateManager>().DebugMode = value; }
            );

            restartButton.onClick.AddListener(
                () => { _gameStateManager.RestartGame(); }
            );

            exitButton.onClick.AddListener(
                () => { _gameStateManager.ExitGame(); }
            );

            masterVolumeSlider.maxValue = 1;
            masterVolumeSlider.minValue = 0;
            masterVolumeSlider.value = AudioListener.volume;
            masterVolumeSlider.onValueChanged.AddListener(
                (value) => { AudioListener.volume = value; }
            );

            controlPanelButton.onClick.AddListener(
                () =>
                {
                    if (controlPanel != null)
                    {
                        controlPanel.SetActive(true);
                    }
                }
            );

            levelsPanelButton.onClick.AddListener(
                () =>
                {
                    if (levelsPanel != null)
                    {
                        levelsPanel.SetActive(true);
                    }
                }
            );
        }

        private void OnDisable()
        {
            if (controlPanel != null && controlPanel.activeInHierarchy)
            {
                controlPanel.SetActive(false);
            }

            if (levelsPanel != null && levelsPanel.activeInHierarchy)
            {
                levelsPanel.SetActive(false);
            }
        }

        private void Update()
        {
            if (Mouse.current.leftButton.isPressed)
            {
                if (controlPanel != null && controlPanel.activeInHierarchy)
                {
                    controlPanel.SetActive(false);
                }

                if (!EventSystem.current.IsPointerOverGameObject())
                {
                    if (levelsPanel != null && levelsPanel.activeInHierarchy)
                    {
                        levelsPanel.SetActive(false);
                    }
                }
            }
        }
    }
}