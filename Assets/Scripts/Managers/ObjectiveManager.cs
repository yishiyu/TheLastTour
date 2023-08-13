using System;
using System.Collections;
using System.Collections.Generic;
using TheLastTour.Event;
using UnityEngine;

namespace TheLastTour.Manager
{
    public class Objective : MonoBehaviour
    {
        public static event Action<Objective> OnObjectiveCreated;
        public static event Action<Objective> OnObjectiveCompleted;

        public bool isComplete = false;

        public string descriptionText = "Objective";

        public virtual void Start()
        {
            OnObjectiveCreated?.Invoke(this);
            UpdateObjective(descriptionText);
        }

        public bool CompleteObjective()
        {
            OnObjectiveCompleted?.Invoke(this);
            isComplete = true;
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
            }
        }
    }
}