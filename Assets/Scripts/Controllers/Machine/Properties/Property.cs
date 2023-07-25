using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace TheLastTour.Controller.Machine
{
    public class PropertyValue<T>
    {
        private T _value;

        public PropertyValue(T value)
        {
            _value = value;
        }

        public T Value
        {
            get { return _value; }
            set
            {
                _value = value;
                OnValueChanged(value);
            }
        }


        public Action<T> OnValueChanged = ((T t) => { });
    }

    public class MachineProperty
    {
        public enum PropertyType
        {
            None,
            Float,
            Bool,
            Key
        }

        public PropertyType type = PropertyType.None;

        public object reference = null;

        public string name = null;

        public MachineProperty(string name, PropertyValue<bool> reference)
        {
            this.name = name;
            this.reference = reference;
            type = PropertyType.Bool;
        }

        public MachineProperty(string name, PropertyValue<float> reference)
        {
            this.name = name;
            this.reference = reference;
            type = PropertyType.Float;
        }

        public MachineProperty(string name, PropertyValue<Key> reference)
        {
            this.name = name;
            this.reference = reference;
            type = PropertyType.Key;
        }
    }

    public class MachinePropertyUI : MonoBehaviour
    {
        public Text PropertyNameText = null;
        public MachineProperty Property = null;

        protected virtual void Start()
        {
            if (Property != null && PropertyNameText != null)
            {
                PropertyNameText.text = Property.name;
            }
            else
            {
                PropertyNameText.text = "error name";
            }
        }
    }
}