using System;
using TheLastTour.Controller.Machine;
using TheLastTour.Event;
using UnityEngine;

namespace TheLastTour.Controller.UI
{
    public class EditorPanelPropertiesBar : MonoBehaviour
    {
        public Transform container;
        public MachinePropertyUI boolPrefab;
        public MachinePropertyUI floatPrefab;
        public MachinePropertyUI keyPrefab;


        #region Event Handlers

        private void OnEnable()
        {
            EventBus.AddListener<SelectedPartChangedEvent>(OnSelectedPartChanged);
        }

        private void OnDisable()
        {
            EventBus.RemoveListener<SelectedPartChangedEvent>(OnSelectedPartChanged);
        }

        private void OnSelectedPartChanged(SelectedPartChangedEvent evt)
        {
            if (evt.CurrentSelectedPart != null)
            {
                SetProperties(evt.CurrentSelectedPart);
            }
            else
            {
                Clear();
            }
        }

        #endregion

        private void Clear()
        {
            for (int i = 0; i < container.childCount; i++)
            {
                Destroy(container.GetChild(i).gameObject);
            }
        }

        private void SetProperties(PartController part)
        {
            Clear();

            if (part == null)
            {
                return;
            }

            for (int i = 0; i < part.Properties.Count; i++)
            {
                MachineProperty property = part.Properties[i];
                MachinePropertyUI ui = null;
                switch (property.PropertyType)
                {
                    case MachineProperty.EPropertyType.Bool:
                        ui = Instantiate(boolPrefab, container);
                        break;
                    case MachineProperty.EPropertyType.Float:
                        ui = Instantiate(floatPrefab, container);
                        break;
                    case MachineProperty.EPropertyType.Key:
                        ui = Instantiate(keyPrefab, container);
                        break;
                }

                if (ui != null)
                {
                    ui.Property = property;
                    ui.name = property.PropertyName;
                }
            }
        }
    }
}