using System;
using TheLastTour.Manager;
using TheLastTour.Utility;
using UnityEngine;
using UnityEngine.InputSystem;


namespace TheLastTour.Controller
{
    public class CameraController : MonoBehaviour
    {
        // 类似 Cinemachine 中的 Follow Target
        // 偏移值为真实焦点位置相对目标在世界坐标系中的偏移
        [SerializeField] public Transform focusTarget = null;

        [SerializeField] public Vector3 focusOffset = Vector3.zero;

        // 用于平滑变换的变量,角度为在世界坐标系中的欧拉角
        // _focusDistanceSmoothSpeed 为平滑变换的瞬时速度
        public float focusDistanceTarget = 10f;
        public float focusDistanceCurrent = 10f;
        private float _focusDistanceSmoothSpeed;
        private const float FocusDistanceMax = 30f;
        private const float FocusDistanceMin = 0.5f;

        private float _focusOffsetMoveSensitive = 0.02f;

        // 防止 Gimbal Lock, 限制 X 轴旋转角度
        public float focusAngleXTarget = 0f;
        public float focusAngleXSmoothTime = 0.1f;
        private float _focusAngleXCurrent = 0f;
        private float _focusAngleXSmoothSpeed = 0;
        private const float FocusAngleXMax = 89f;
        private const float FocusAngleXMin = -89f;

        public float focusAngleYTarget = 0f;
        public float focusAngleYSmoothTime = 0.1f;
        private float _focusAngleYCurrent = 0f;
        private float _focusAngleYSmoothSpeed = 0;

        // 相机自身姿态(由上面的变量计算得到,相机 roll 恒定为0)
        Quaternion _cameraRotation = Quaternion.identity;
        Vector3 _cameraPosition = Vector3.zero;


        private IGameStateManager _gameStateManager;
        private InputActions _inputActions;

        public void Start()
        {
            _gameStateManager = TheLastTourArchitecture.Instance.GetManager<IGameStateManager>();
            _inputActions = TheLastTourArchitecture.Instance.GetUtility<IInputUtility>().GetInputActions();
        }


        public void Update()
        {
            switch (_gameStateManager.GameState)
            {
                case EGameState.Edit:
                    UpdateEditCamera();
                    break;
                case EGameState.Play:
                    UpdatePlayCamera();
                    break;
                case EGameState.Pause:
                    break;
                case EGameState.GameOver:
                    break;
            }
        }

        private void UpdateEditCamera()
        {
            if (focusTarget == null ||
                _inputActions == null ||
                !_inputActions.CameraControl.EnableCameraControl.IsPressed())
            {
                return;
            }

            // 获取鼠标输入
            float scrollDelta = Mouse.current.scroll.ReadValue().y;
            float mouseXDelta = Mouse.current.delta.x.ReadValue();
            float mouseYDelta = Mouse.current.delta.y.ReadValue();

            // 获取键盘输入
            float keyboardXDelta = Keyboard.current[Key.A].ReadValue() - Keyboard.current[Key.D].ReadValue();
            float keyboardYDelta = Keyboard.current[Key.S].ReadValue() - Keyboard.current[Key.W].ReadValue();
            float keyboardZDelta = Keyboard.current[Key.Q].ReadValue() - Keyboard.current[Key.E].ReadValue();


            // 更新目标值
            Vector3 forward = Vector3.Scale(transform.forward, new Vector3(1, 0, 1)).normalized;
            Vector3 right = Vector3.Scale(transform.right, new Vector3(1, 0, 1)).normalized;
            Vector3 up = Vector3.up;
            focusOffset -= keyboardYDelta * _focusOffsetMoveSensitive * forward +
                           keyboardXDelta * _focusOffsetMoveSensitive * right +
                           keyboardZDelta * _focusOffsetMoveSensitive * up;
            focusAngleXTarget = Mathf.Clamp(focusAngleXTarget - mouseYDelta, FocusAngleXMin, FocusAngleXMax);
            focusAngleYTarget = (focusAngleYTarget + mouseXDelta) % 360;
            focusDistanceTarget = Mathf.Clamp(focusDistanceTarget - scrollDelta, FocusDistanceMin, FocusDistanceMax);


            // 平滑变换
            _focusAngleXCurrent = Mathf.SmoothDampAngle(_focusAngleXCurrent, focusAngleXTarget,
                ref _focusAngleXSmoothSpeed, focusAngleXSmoothTime);
            _focusAngleYCurrent = Mathf.SmoothDampAngle(_focusAngleYCurrent, focusAngleYTarget,
                ref _focusAngleYSmoothSpeed, focusAngleYSmoothTime);
            focusDistanceCurrent = Mathf.SmoothDamp(focusDistanceCurrent, focusDistanceTarget,
                ref _focusDistanceSmoothSpeed, 0.1f);


            // 计算相机姿态
            _cameraRotation = Quaternion.Euler(_focusAngleXCurrent, _focusAngleYCurrent, 0);
            _cameraPosition = focusTarget.position - _cameraRotation * Vector3.forward * focusDistanceCurrent +
                              focusOffset;

            // 更新相机姿态
            transform.rotation = _cameraRotation;
            transform.position = _cameraPosition;
        }

        private void UpdatePlayCamera()
        {
        }
    }
}