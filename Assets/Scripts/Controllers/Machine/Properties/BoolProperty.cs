using Unity.Properties;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastTour.Controller.Machine
{
    public class BoolProperty : MachinePropertyUI
    {
        public Toggle toggle;

        protected override void Start()
        {
            base.Start();

            if (toggle != null && Property != null &&
                Property.reference != null &&
                Property.type == MachineProperty.PropertyType.Bool)
            {
                toggle.isOn = (Property.reference as PropertyValue<bool>).Value;

                toggle.onValueChanged.AddListener((value) =>
                {
                    (Property.reference as PropertyValue<bool>).Value = value;
                });
            }
        }
    }
}