using UnityEngine;
using UnityEngine.UI;

namespace TheLastTour.Controller.Machine
{
    public class FloatProperty : MachinePropertyUI
    {
        public InputField inputField;

        protected override void Start()
        {
            base.Start();

            if (inputField != null && Property != null &&
                Property.reference != null &&
                Property.type == MachineProperty.PropertyType.Float)
            {
                inputField.text = (Property.reference as PropertyValue<float>).Value.ToString();
                inputField.contentType = InputField.ContentType.DecimalNumber;

                inputField.onSubmit.AddListener((value) =>
                {
                    float result = 0;
                    if (float.TryParse(value, out result))
                    {
                        (Property.reference as PropertyValue<float>).Value = result;
                    }
                });
            }
        }
    }
}