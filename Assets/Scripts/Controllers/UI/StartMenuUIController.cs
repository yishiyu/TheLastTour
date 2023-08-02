using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


namespace TheLastTour.Controller.UI
{
    public class StartMenuUIController : MonoBehaviour
    {
        public Button playButton;
        public Button quitButton;
        public Image fadeImage;


        private void Start()
        {
            playButton.onClick.AddListener(() =>
            {
                Debug.Log("Game Start");
                StartCoroutine(LoadGameWithFadeOut("GameScene_Intro"));
            });
            quitButton.onClick.AddListener(() =>
            {
                Debug.Log("Game Quit");
#if UNITY_EDITOR
                // Application.Quit() does not work in the editor so
                // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            });
        }

        private IEnumerator LoadGameWithFadeOut(string sceneName)
        {
            float time = 0;
            while (time < 0.95)
            {
                time += Time.deltaTime * 0.5f;
                fadeImage.color = new Color(0, 0, 0, time);
                yield return null;
            }

            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
    }
}