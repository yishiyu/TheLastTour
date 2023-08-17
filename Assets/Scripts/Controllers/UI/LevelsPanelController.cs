using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheLastTour.Controller.UI
{
    public class LevelsPanelController : MonoBehaviour
    {
        public void OpenLevel(string levelName)
        {
            SceneManager.LoadScene(levelName);
        }
    }
}