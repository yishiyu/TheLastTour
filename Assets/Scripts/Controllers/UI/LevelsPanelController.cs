using System.Collections;
using System.Collections.Generic;
using TheLastTour.Manager;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheLastTour.Controller.UI
{
    public class LevelsPanelController : MonoBehaviour
    {
        public void OpenLevel(string levelName)
        {
            var gameStateManager = TheLastTourArchitecture.Instance.GetManager<IGameStateManager>();
            if (gameStateManager != null && gameStateManager.GameState == EGameState.Pause)
            {
                // 恢复游戏
                gameStateManager.ChaneToPreviousState();
            }

            SceneManager.LoadScene(levelName);
        }
    }
}