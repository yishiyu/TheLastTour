using System;
using System.Collections;
using System.Collections.Generic;
using TheLastTour.Controller.Machine;
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

    public enum EEditState
    {
        Placing,
        Selecting
    }

    public interface IGameStateManager : IManager
    {
        public EGameState GameState { get; set; }

        public EEditState EditState { get; set; }

        public void ChaneToPreviousState();
        
        public bool DebugMode { get; set; }
    }

    public class GameStateManager : IGameStateManager
    {
        // 游戏状态
        private EGameState _gameState;
        private EGameState _previousGameState;

        public void ChaneToPreviousState()
        {
            ((IGameStateManager)this).GameState = _previousGameState;
        }

        public bool DebugMode { get; set; }

        EGameState IGameStateManager.GameState
        {
            get => _gameState;
            set
            {
                if (_gameState != value)
                {
                    _previousGameState = _gameState;
                    GameEvents.GameStateChangedEvent.PreviousState = _gameState;
                    GameEvents.GameStateChangedEvent.CurrentState = value;
                    _gameState = value;
                    EventBus.Invoke(GameEvents.GameStateChangedEvent);
                }
            }
        }

        private EEditState _editState;

        EEditState IGameStateManager.EditState
        {
            get => _editState;
            set
            {
                if (_editState != value)
                {
                    GameEvents.EditStateChangedEvent.PreviousState = _editState;
                    GameEvents.EditStateChangedEvent.CurrentState = value;
                    _editState = value;
                    EventBus.Invoke(GameEvents.EditStateChangedEvent);
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