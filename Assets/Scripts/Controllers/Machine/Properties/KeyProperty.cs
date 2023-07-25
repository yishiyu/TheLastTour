using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TheLastTour.Controller.Machine
{
    public class KeyProperty : MachinePropertyUI
    {
        public Button button;
        bool inputKeyBinding = false;

        protected override void Start()
        {
            base.Start();

            if (button != null && Property != null &&
                Property.reference != null &&
                Property.type == MachineProperty.PropertyType.Key)
            {
                button.GetComponentInChildren<Text>().text =
                    (Property.reference as PropertyValue<Key>).AsValue.ToString();


                button.onClick.AddListener(() =>
                {
                    button.GetComponentInChildren<Text>().text = "按下任意键";
                    inputKeyBinding = true;
                });
            }
        }


        private void Update()
        {
            if (inputKeyBinding && Property != null && Property.reference != null &&
                Property.type == MachineProperty.PropertyType.Key)
            {
                if (Keyboard.current.backspaceKey.isPressed || Keyboard.current.escapeKey.isPressed)
                {
                    (Property.reference as PropertyValue<Key>).AsRef = Key.None;
                    inputKeyBinding = false;
                    button.GetComponentInChildren<Text>().text =
                        (Property.reference as PropertyValue<Key>).AsValue.ToString();
                }

                if (Keyboard.current.anyKey.isPressed)
                {
                    foreach (Key keyCode in Enum.GetValues(typeof(Key)))
                    {
                        if (Keyboard.current[keyCode].isPressed)
                        {
                            (Property.reference as PropertyValue<Key>).AsRef = keyCode;
                            inputKeyBinding = false;
                            button.GetComponentInChildren<Text>().text =
                                (Property.reference as PropertyValue<Key>).AsValue.ToString();
                            break;
                        }
                    }
                }
            }
        }
    }
}