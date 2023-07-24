using System;
using System.Collections.Generic;
using TheLastTour.Event;
using TheLastTour.Manager;
using UnityEngine;

namespace TheLastTour.Controller.Machine
{
    [RequireComponent(typeof(Rigidbody))]
    public class MachineController : MonoBehaviour
    {
        private Rigidbody _rigidbody;

        private List<PartController> _parts = new List<PartController>();

        private void OnEnable()
        {
            EventBus.AddListener<GameStateChangedEvent>(OnGameStateChanged);
        }


        private void OnDisable()
        {
            EventBus.RemoveListener<GameStateChangedEvent>(OnGameStateChanged);
        }

        private void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void OnGameStateChanged(GameStateChangedEvent evt)
        {
            switch (evt.CurrentState)
            {
                case EGameState.Play:
                    // TODO 保存模型
                    // TODO 开启模型物理模拟
                    break;
                case EGameState.Edit:
                    // TODO 读取模型
                    // TODO 暂停模型物理模拟
                    break;
                default:
                    break;
            }
        }


        public void UpdateMachineMass()
        {
            float mass = 0;
            Vector3 centerOfMass = Vector3.zero;
            foreach (var part in _parts)
            {
                mass += part.mass;
                centerOfMass += part.mass * part.transform.position;
            }

            _rigidbody.mass = mass;
            _rigidbody.centerOfMass = centerOfMass / mass;
        }
    }
}