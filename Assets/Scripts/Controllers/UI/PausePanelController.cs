using System.Collections;
using System.Collections.Generic;
using TheLastTour.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastTour.Controller.UI
{
    public class PausePanelController : MonoBehaviour
    {
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
        }
    }
}