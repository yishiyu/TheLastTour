using System;
using Unity.Properties;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UI;

namespace TheLastTour.Controller.Machine
{
    public class KeyProperty : MachinePropertyUI
    {
        public Button button;

        private bool _inputBinding = false;
        private PropertyValue<Key> _property;

        protected override void Start()
        {
            base.Start();
            _property = Property.Reference as PropertyValue<Key>;


            if (_property != null && button != null)
            {
                button.GetComponentInChildren<Text>().text = _property.Value.ToString();

                button.onClick.AddListener(() =>
                {
                    button.GetComponentInChildren<Text>().text = "Press any key";
                    _inputBinding = true;
                });
            }
        }


        private void Update()
        {
            if (_inputBinding && _property != null && button != null)
            {
                if (Keyboard.current.backspaceKey.isPressed || Keyboard.current.escapeKey.isPressed)
                {
                    _property.Value = Key.None;
                    _inputBinding = false;
                    button.GetComponentInChildren<Text>().text = _property.Value.ToString();
                }


                if (Keyboard.current.anyKey.isPressed)
                {
                    _property.Value = Key.None;

                    foreach (Key key in Enum.GetValues(typeof(Key)))
                    {
                        if (key == Key.None)
                        {
                            continue;
                        }

                        if (Keyboard.current[key].isPressed)
                        {
                            _property.Value = key;
                            break;
                        }
                    }

                    button.GetComponentInChildren<Text>().text = _property.Value.ToString();
                    _inputBinding = false;
                }
            }
        }
    }
}