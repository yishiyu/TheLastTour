using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UI;

namespace TheLastTour.Controller.Machine
{
    public class KeyProperty : MachinePropertyUI
    {
        public Button button;
        private bool inputBinding = false;

        protected override void Start()
        {
            base.Start();

            if (button != null && Property != null &&
                Property.reference != null &&
                Property.type == MachineProperty.PropertyType.Key)
            {
                button.GetComponentInChildren<Text>().text =
                    (Property.reference as PropertyValue<Key>).Value.ToString();

                button.onClick.AddListener(() =>
                {
                    button.GetComponentInChildren<Text>().text = "Press any key";
                    inputBinding = true;
                });
            }
        }


        private void Update()
        {
            if (inputBinding && Property != null && Property.reference != null &&
                Property.type == MachineProperty.PropertyType.Key)
            {
                if (Keyboard.current.backspaceKey.isPressed || Keyboard.current.escapeKey.isPressed)
                {
                    (Property.reference as PropertyValue<Key>).Value = Key.None;
                    inputBinding = false;
                    button.GetComponentInChildren<Text>().text =
                        (Property.reference as PropertyValue<Key>).Value.ToString();
                }


                if (Keyboard.current.anyKey.isPressed)
                {
                    (Property.reference as PropertyValue<Key>).Value = Key.None;

                    foreach (Key key in Enum.GetValues(typeof(Key)))
                    {
                        if (key == Key.None)
                        {
                            continue;
                        }

                        if (Keyboard.current[key].isPressed)
                        {
                            (Property.reference as PropertyValue<Key>).Value = key;

                            break;
                        }
                    }

                    button.GetComponentInChildren<Text>().text =
                        (Property.reference as PropertyValue<Key>).Value.ToString();
                    inputBinding = false;
                }
            }
        }
    }
}