using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace TheLastTour.Controller.Machine
{
    public class PropertyValue<T>
    {
        private T Value;

        public PropertyValue(T value)
        {
            Value = value;
        }

        public T AsValue
        {
            get { return Value; }
        }

        public ref T AsRef
        {
            get { return ref Value; }
        }
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