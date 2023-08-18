using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheLastTour.Controller.Objective
{
    [RequireComponent(typeof(Collider))]
    public class RacingCheckPoint : MonoBehaviour
    {
        public ObjectiveRacing objectiveRacing;

        public bool isArrived = true;

        public void EnableCheckPoint()
        {
            isArrived = false;
        }

        public void DisableCheckPoint()
        {
            isArrived = true;
        }

        public void OnTriggerEnter(Collider other)
        {
            if (!isArrived)
            {
                if (other.gameObject.CompareTag("Player"))
                {
                    isArrived = true;
                    objectiveRacing.CheckPointArrived(this);
                }
            }
        }
    }
}