using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheLastTour.Manager;

namespace TheLastTour.Controller.Objective
{
    [RequireComponent(typeof(Collider))]
    public class ObjectiveReachPoint : Manager.Objective
    {
        public string reachPointDescription = "Reach Point";
        Collider _collider;

        Collider ObjectiveCollider
        {
            get
            {
                if (_collider == null)
                {
                    _collider = GetComponent<Collider>();
                }

                return _collider;
            }
        }


        private void OnTriggerEnter(Collider collider)
        {
            if (!isComplete && collider.gameObject.CompareTag("Player"))
            {
                CompleteObjective();
                UpdateObjective("Arrive at " + reachPointDescription);
            }
        }
    }
}