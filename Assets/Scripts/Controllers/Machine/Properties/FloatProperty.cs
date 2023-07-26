using UnityEngine;
using UnityEngine.UI;

namespace TheLastTour.Controller.Machine
{
    public class FloatProperty : MachinePropertyUI
    {
        public InputField inputField;
        private PropertyValue<float> _property;

        protected override void Start()
        {
            base.Start();
            _property = Property.Reference as PropertyValue<float>;

            if (inputField != null && _property != null)
            {
                inputField.text = _property.Value.ToString();
                inputField.contentType = InputField.ContentType.DecimalNumber;

                inputField.onSubmit.AddListener((value) =>
                {
                    if (float.TryParse(value, out var result))
                    {
                        _property.Value = result;
                    }
                });
            }
        }
    }
}