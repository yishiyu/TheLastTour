using System;
using System.Collections;
using System.Collections.Generic;
using TheLastTour.Event;
using UnityEngine;

namespace TheLastTour.Manager
{
    public class Objective : MonoBehaviour
    {
        public AudioClip objectiveCompleteSound;
        public float objectiveCompleteSoundVolume = 0.3f;

        public static event Action<Objective> OnObjectiveCreated;
        public static event Action<Objective> OnObjectiveCompleted;

        public bool isComplete = false;

        public string descriptionText = "Objective";


        private IAudioManager _audioManager;

        public virtual void Start()
        {
            _audioManager = TheLastTourArchitecture.Instance.GetManager<IAudioManager>();

            OnObjectiveCreated?.Invoke(this);
            UpdateObjective(descriptionText);
        }

        private void OnDestroy()
        {
            OnObjectiveCompleted?.Invoke(this);
        }

        public bool CompleteObjective()
        {
            isComplete = true;
            UpdateObjective(descriptionText);
            OnObjectiveCompleted?.Invoke(this);
            // 播放音效
            if (objectiveCompleteSound != null)
            {
                _audioManager.PlaySound(objectiveCompleteSound, objectiveCompleteSoundVolume, false);
            }

            return true;
        }

        public void UpdateObjective(string description)
        {
            descriptionText = description;
            GameEvents.ObjectiveUpdateEvent.ObjectiveObj = this;
            GameEvents.ObjectiveUpdateEvent.IsComplete = isComplete;
            EventBus.Invoke(GameEvents.ObjectiveUpdateEvent);
        }
    }


    public interface IObjectiveManager : IManager
    {
        public void RegisterObjective(Objective objective);

        public void UnregisterObjective(Objective objective);

        public List<Objective> Objectives { get; }
    }

    public class ObjectiveManager : IObjectiveManager
    {
        private List<Objective> _objectives = new List<Objective>();

        public List<Objective> Objectives
        {
            get => _objectives;
        }

        private bool ObjectiveCompleted
        {
            get => (_objectives.Count == 0);
        }

        public void Init(IArchitecture architecture)
        {
            Debug.Log("ObjectiveManager Init");

            Objective.OnObjectiveCreated += RegisterObjective;
            Objective.OnObjectiveCompleted += UnregisterObjective;
        }

        void OnDestroy()
        {
            Objective.OnObjectiveCreated -= RegisterObjective;
            Objective.OnObjectiveCompleted -= UnregisterObjective;
        }

        public void RegisterObjective(Objective objective)
        {
            _objectives.Add(objective);
        }

        public void UnregisterObjective(Objective objective)
        {
            _objectives.Remove(objective);
            if (ObjectiveCompleted)
            {
                EventBus.Invoke(GameEvents.AllObjectivesCompletedEvent);
                TheLastTourArchitecture.Instance.GetManager<IGameStateManager>().GameState = EGameState.GameOver;
            }
        }
    }
}