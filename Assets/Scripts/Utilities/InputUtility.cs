using UnityEngine;

namespace TheLastTour.Utility
{
    public interface IInputUtility : IUtility
    {
        public InputActions GetInputActions();
    }


    public class InputUtility : IInputUtility
    {
        private InputActions _inputActions;


        public void Init(IArchitecture architecture)
        {
            Debug.Log("InputUtility Init");
            _inputActions = new InputActions();

            _inputActions.Enable();
        }

        public InputActions GetInputActions()
        {
            return _inputActions;
        }
    }
}