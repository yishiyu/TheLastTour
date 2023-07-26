using Unity.Properties;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastTour.Controller.Machine
{
    public class BoolProperty : MachinePropertyUI
    {
        public Toggle toggle;
        private PropertyValue<bool> _property;

        protected override void Start()
        {
            base.Start();
            _property = Property.Reference as PropertyValue<bool>;

            if (toggle != null && _property != null)
            {
                toggle.isOn = _property.Value;

                toggle.onValueChanged.AddListener((value) => { _property.Value = value; });
            }
        }
    }
}