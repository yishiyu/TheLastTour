using System;
using System.Collections;
using System.Collections.Generic;
using TheLastTour.Event;
using TheLastTour.Manager;
using Unity.VisualScripting;
using UnityEngine;


namespace TheLastTour.Controller.Objective
{
    public class ObjectiveSpeed : Manager.Objective
    {
        public float targetSpeed = 10f;
        private Rigidbody _playerRigidbody;

        public override void Start()
        {
            base.Start();
            EventBus.AddListener<GameStateChangedEvent>(OnGameStateChanged);
        }

        public void OnDestroy()
        {
            EventBus.RemoveListener<GameStateChangedEvent>(OnGameStateChanged);
        }

        private void OnGameStateChanged(GameStateChangedEvent evt)
        {
            if (evt.CurrentState == EGameState.Play && evt.PreviousState == EGameState.Edit)
            {
                // 获取玩家
                _playerRigidbody = TheLastTourArchitecture.Instance
                    .GetManager<IMachineManager>().GetCorePart().GetSimulatorRigidbody();
            }
        }

        private void Update()
        {
            if (isComplete) return;
            if (_playerRigidbody == null) return;

            float speed = _playerRigidbody.velocity.magnitude;
            descriptionText = "达到目标速度: " + speed + "/" + targetSpeed;
            UpdateObjective(descriptionText);

            if (speed >= targetSpeed)
            {
                descriptionText = "达到目标速度!";
                CompleteObjective();
            }
        }
    }
}