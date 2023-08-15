using System;
using System.Collections;
using System.Collections.Generic;
using TheLastTour.Event;
using TheLastTour.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastTour.Controller.UI
{
    public class ObjectivePanel : MonoBehaviour
    {
        public GameObject objectiveUIContainer;
        public GameObject objectiveContainer;
        public ObjectiveUIController objectivePrefab;

        private IObjectiveManager _objectiveManager;
        private IGameStateManager _gameStateManager;

        private Dictionary<Manager.Objective, ObjectiveUIController> _objectiveUIControllers =
            new Dictionary<Manager.Objective, ObjectiveUIController>();

        public GameObject objectiveCompleteInfo;
        public Text objectiveCompleteText;

        private void Start()
        {
            _objectiveManager = TheLastTourArchitecture.Instance.GetManager<IObjectiveManager>();
            _gameStateManager = TheLastTourArchitecture.Instance.GetManager<IGameStateManager>();
            objectiveUIContainer.SetActive(false);
        }

        private void OnEnable()
        {
            EventBus.AddListener<ObjectiveUpdateEvent>(OnObjectiveUpdate);
            EventBus.AddListener<AllObjectivesCompletedEvent>(OnAllObjectivesCompleted);
            EventBus.AddListener<GameStateChangedEvent>(OnGameStateChanged);

            for (int i = 0; i < objectiveContainer.transform.childCount; i++)
            {
                Destroy(objectiveContainer.transform.GetChild(i).gameObject);
            }

            _objectiveUIControllers.Clear();

            if (_objectiveManager != null)
            {
                foreach (Manager.Objective objective in _objectiveManager.Objectives)
                {
                    ObjectiveUIController controller = Instantiate(objectivePrefab, objectiveContainer.transform);
                    _objectiveUIControllers.Add(objective, controller);
                    controller.UpdateObjective(objective.descriptionText);
                }
            }
        }

        private void OnGameStateChanged(GameStateChangedEvent obj)
        {
            switch (obj.CurrentState)
            {
                case EGameState.Edit:
                    objectiveUIContainer.SetActive(false);
                    break;
                case EGameState.Play:
                    if (_objectiveUIControllers.Count > 0)
                    {
                        objectiveUIContainer.SetActive(true);
                    }

                    break;
            }
        }

        private void OnDisable()
        {
            EventBus.RemoveListener<ObjectiveUpdateEvent>(OnObjectiveUpdate);
            EventBus.RemoveListener<AllObjectivesCompletedEvent>(OnAllObjectivesCompleted);
            EventBus.RemoveListener<GameStateChangedEvent>(OnGameStateChanged);
        }

        private void OnAllObjectivesCompleted(AllObjectivesCompletedEvent obj)
        {
            objectiveUIContainer.SetActive(false);
        }


        private void OnObjectiveUpdate(ObjectiveUpdateEvent obj)
        {
            if (obj.IsComplete)
            {
                if (_objectiveUIControllers.ContainsKey(obj.ObjectiveObj))
                {
                    ObjectiveUIController controller = _objectiveUIControllers[obj.ObjectiveObj];
                    _objectiveUIControllers.Remove(obj.ObjectiveObj);
                    controller.CompleteObjective();

                    StartCoroutine(CompleteObjective(obj));
                }
            }
            else
            {
                if (_objectiveUIControllers.ContainsKey(obj.ObjectiveObj))
                {
                    ObjectiveUIController controller = _objectiveUIControllers[obj.ObjectiveObj];
                    // _objectiveUIControllers.Remove(obj.ObjectiveObj);
                    controller.UpdateObjective(obj.ObjectiveObj.descriptionText);
                }
                else
                {
                    ObjectiveUIController controller = Instantiate(objectivePrefab, objectiveContainer.transform);
                    controller.UpdateObjective(obj.ObjectiveObj.descriptionText);
                    _objectiveUIControllers.Add(obj.ObjectiveObj, controller);

                    if (!objectiveContainer.activeSelf &&
                        _gameStateManager != null &&
                        _gameStateManager.GameState == EGameState.Play)
                    {
                        objectiveUIContainer.SetActive(true);
                    }
                }
            }
        }

        private IEnumerator CompleteObjective(ObjectiveUpdateEvent obj)
        {
            objectiveCompleteInfo.SetActive(true);
            objectiveCompleteText.text = obj.ObjectiveObj.descriptionText;

            yield return new WaitForSeconds(3f);

            objectiveCompleteInfo.SetActive(false);
        }
    }
}