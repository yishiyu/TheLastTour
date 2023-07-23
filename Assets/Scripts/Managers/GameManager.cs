using System.Collections;
using System.Collections.Generic;
using TheLastTour.Event;
using UnityEngine;

namespace TheLastTour.Manager
{
    public enum EGameState
    {
        Edit,
        Play,
        Pause,
        GameOver
    }

    public interface IGameManager : IManager
    {
        public EGameState GameState { get; protected set; }
    }

    public class GameManager : IGameManager
    {
        private EGameState _gameState;

        EGameState IGameManager.GameState
        {
            get => _gameState;
            set
            {
                if (_gameState != value)
                {
                    GameEvents.GameStateChangedEvent.PreviousState = _gameState;
                    GameEvents.GameStateChangedEvent.CurrentState = value;
                    _gameState = value;
                    EventBus.Invoke(GameEvents.GameStateChangedEvent);
                }
            }
        }

        public void Init(IArchitecture architecture)
        {
            _gameState = EGameState.Edit;
            Debug.Log("GameManager Init");
        }
    }
}