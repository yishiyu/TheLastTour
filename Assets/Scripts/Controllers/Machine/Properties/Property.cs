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
        public enum EPropertyType
        {
            None,
            Float,
            Bool,
            Key
        }

        public readonly EPropertyType PropertyType;
        public readonly object Reference = null;
        public readonly string PropertyName = null;

        public MachineProperty(string name, PropertyValue<bool> reference)
        {
            PropertyName = name;
            Reference = reference;
            PropertyType = EPropertyType.Bool;
        }

        public MachineProperty(string name, PropertyValue<float> reference)
        {
            PropertyName = name;
            Reference = reference;
            PropertyType = EPropertyType.Float;
        }

        public MachineProperty(string name, PropertyValue<Key> reference)
        {
            PropertyName = name;
            Reference = reference;
            PropertyType = EPropertyType.Key;
        }
    }

    public class MachinePropertyUI : MonoBehaviour
    {
        public Text propertyNameText = null;
        public MachineProperty Property = null;

        protected virtual void Start()
        {
            if (Property != null && propertyNameText != null)
            {
                propertyNameText.text = Property.PropertyName;
            }
            else
            {
                propertyNameText.text = "invalid property";
            }
        }
    }
}