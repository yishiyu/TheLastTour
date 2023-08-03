using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TheLastTour.Controller.UI
{
    public class IntroUIController : MonoBehaviour
    {
        public Image fadeImage;

        public List<GameObject> intros;
        private int _currentIntroIndex = 0;

        public void ChangeIntro()
        {
            Debug.Log("Change Intro");

            StartCoroutine(FadeOutToNextIntro());
        }

        IEnumerator FadeOutToNextIntro()
        {
            float alpha = 0;
            while (alpha < 1)
            {
                alpha += Time.deltaTime * 0.8f;
                fadeImage.color = new Color(0, 0, 0, alpha);
                yield return null;
            }

            yield return new WaitForSeconds(0.5f);

            if (_currentIntroIndex >= 0 && _currentIntroIndex < intros.Count)
            {
                intros[_currentIntroIndex].gameObject.SetActive(false);
            }

            _currentIntroIndex++;

            if (_currentIntroIndex >= 0 && _currentIntroIndex < intros.Count)
            {
                intros[_currentIntroIndex].gameObject.SetActive(true);
            }

            while (alpha > 0)
            {
                alpha -= Time.deltaTime * 0.8f;
                fadeImage.color = new Color(0, 0, 0, alpha);
                yield return null;
            }
        }

        public void EnterNextScene()
        {
            Debug.Log("Enter Next Scene");
            SceneManager.LoadScene("LevelScene_01");
        }
    }
}