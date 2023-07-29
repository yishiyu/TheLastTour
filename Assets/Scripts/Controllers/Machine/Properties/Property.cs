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

        public JsonMachineProperty Serialize()
        {
            JsonMachineProperty jsonMachineProperty = new JsonMachineProperty();
            jsonMachineProperty.PropertyName = PropertyName;
            switch (PropertyType)
            {
                case EPropertyType.Float:
                    jsonMachineProperty.Value = ((PropertyValue<float>)Reference).Value;
                    break;
                case EPropertyType.Bool:
                    jsonMachineProperty.Value = ((PropertyValue<bool>)Reference).Value ? 1 : 0;
                    break;
                case EPropertyType.Key:
                    jsonMachineProperty.Value = (int)((PropertyValue<Key>)Reference).Value;
                    break;
                default:
                    Debug.LogError("invalid property type");
                    break;
            }

            return jsonMachineProperty;
        }

        public void Deserialize(JsonMachineProperty jsonMachineProperty)
        {
            switch (PropertyType)
            {
                case EPropertyType.Float:
                    ((PropertyValue<float>)Reference).Value = jsonMachineProperty.Value;
                    break;
                case EPropertyType.Bool:
                    ((PropertyValue<bool>)Reference).Value = jsonMachineProperty.Value > 0;
                    break;
                case EPropertyType.Key:
                    ((PropertyValue<Key>)Reference).Value = (Key)jsonMachineProperty.Value;
                    break;
                default:
                    Debug.LogError("invalid property type");
                    break;
            }
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

    public struct JsonMachineProperty
    {
        public string PropertyName;
        public float Value;
    }
}