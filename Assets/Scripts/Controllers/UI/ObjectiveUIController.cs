using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastTour.Controller.UI
{
    public class ObjectiveUIController : MonoBehaviour
    {
        public Text descriptionText;

        public void CompleteObjective()
        {
            Debug.Log("Objective Complete");
            Destroy(gameObject);
        }

        public void UpdateObjective(string description)
        {
            descriptionText.text = description;
            Debug.Log("Objective Update: " + description);
        }
    }
}