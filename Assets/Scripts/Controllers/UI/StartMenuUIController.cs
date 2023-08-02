using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace TheLastTour.Controller.UI
{
    public class StartMenuUIController : MonoBehaviour
    {
        public Button PlayButton;
        public Button QuitButton;

        private void Start()
        {
            PlayButton.onClick.AddListener(() => { Debug.Log("Game Start"); });
            QuitButton.onClick.AddListener(() => { Debug.Log("Game Quit"); });
        }
    }
}