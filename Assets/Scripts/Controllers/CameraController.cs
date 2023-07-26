using System;
using TheLastTour.Event;
using TheLastTour.Manager;
using TheLastTour.Utility;
using UnityEngine;
using UnityEngine.InputSystem;


namespace TheLastTour.Controller
{
    public class CameraController : MonoBehaviour
    {
        #region Instance

        private static CameraController _instance;

        public static CameraController Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<CameraController>();
                }

                return _instance;
            }
        }

        #endregion


        #region CameraConfig

        // 偏移值为真实焦点位置相对目标在世界坐标系中的偏移
        public Transform focusTarget = null;
        public Vector3 focusOffset = Vector3.zero;

        // 焦点距离
        public float focusDistanceTarget = 10f;
        public float focusDistanceMax = 50f;
        public float focusDistanceMin = 3f;

        // 焦点角度
        public float focusAngleXTarget = 0f;
        public float focusAngleYTarget = 0f;

        // 焦点角度变化平滑时间
        public float focusAngleXSmoothTime = 0.1f;
        public float focusAngleYSmoothTime = 0.1f;

        // 防止 Gimbal Lock, 限制 X 轴旋转角度
        public float focusAngleXMax = 89f;
        public float focusAngleXMin = -89f;

        // 灵敏度
        public float focusZoomSensitive = 0.01f;
        public float focusOffsetMoveSensitive = 0.02f;
        public float focusAngleXSensitive = 0.3f;
        public float focusAngleYSensitive = 0.3f;

        #endregion

        // 平滑时的当前值
        private float _focusDistanceCurrent = 10f;
        private float _focusAngleXCurrent = 0f;
        private float _focusAngleYCurrent = 0f;

        // 平滑时的瞬时变化速度
        private float _focusDistanceSmoothSpeed;
        private float _focusAngleXSmoothSpeed = 0;
        private float _focusAngleYSmoothSpeed = 0;


        // 计算得到的当前相机姿态
        Quaternion _cameraRotation = Quaternion.identity;
        Vector3 _cameraPosition = Vector3.zero;


        private IGameStateManager _gameStateManager;
        private InputActions _inputActions;

        #region Initialization

        public void Start()
        {
            _gameStateManager = TheLastTourArchitecture.Instance.GetManager<IGameStateManager>();
            _inputActions = TheLastTourArchitecture.Instance.GetUtility<IInputUtility>().GetInputActions();
        }

        #endregion

        #region Event Handlers

        private void OnEnable()
        {
            EventBus.AddListener<FocusOnTargetEvent>(OnFocusOnTargetEvent);
        }

        private void OnDisable()
        {
            EventBus.RemoveListener<FocusOnTargetEvent>(OnFocusOnTargetEvent);
        }

        private void OnFocusOnTargetEvent(FocusOnTargetEvent evt)
        {
            focusTarget = evt.Target;
            focusOffset = Vector3.zero;
            focusDistanceTarget = 10f;
        }

        #endregion


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
            // 受控输入
            float scrollDelta = 0;
            float mouseXDelta = 0;
            float mouseYDelta = 0;
            float keyboardXDelta = 0;
            float keyboardYDelta = 0;
            float keyboardZDelta = 0;
            if (_inputActions.CameraControl.EnableCameraControl.IsPressed())
            {
                // 鼠标输入
                scrollDelta = Mouse.current.scroll.ReadValue().y * focusZoomSensitive;
                mouseXDelta = Mouse.current.delta.x.ReadValue() * focusAngleXSensitive;
                mouseYDelta = Mouse.current.delta.y.ReadValue() * focusAngleYSensitive;

                // 获取键盘输入
                keyboardXDelta = Keyboard.current[Key.A].ReadValue() - Keyboard.current[Key.D].ReadValue();
                keyboardYDelta = Keyboard.current[Key.S].ReadValue() - Keyboard.current[Key.W].ReadValue();
                keyboardZDelta = Keyboard.current[Key.Q].ReadValue() - Keyboard.current[Key.E].ReadValue();
            }


            // 更新目标值
            Vector3 forward = Vector3.Scale(transform.forward, new Vector3(1, 0, 1)).normalized;
            Vector3 right = Vector3.Scale(transform.right, new Vector3(1, 0, 1)).normalized;
            Vector3 up = Vector3.up;
            focusOffset -= keyboardYDelta * focusOffsetMoveSensitive * forward +
                           keyboardXDelta * focusOffsetMoveSensitive * right +
                           keyboardZDelta * focusOffsetMoveSensitive * up;

            // 偏移过大时失去焦点
            if (focusOffset.magnitude > 3)
            {
                focusTarget = null;
            }

            focusAngleXTarget = Mathf.Clamp(focusAngleXTarget - mouseYDelta, focusAngleXMin, focusAngleXMax);
            focusAngleYTarget = (focusAngleYTarget + mouseXDelta) % 360;
            focusDistanceTarget =
                Mathf.Clamp(focusDistanceTarget - scrollDelta, focusDistanceMin, focusDistanceMax);


            // 平滑变换
            _focusAngleXCurrent = Mathf.SmoothDampAngle(_focusAngleXCurrent, focusAngleXTarget,
                ref _focusAngleXSmoothSpeed, focusAngleXSmoothTime);
            _focusAngleYCurrent = Mathf.SmoothDampAngle(_focusAngleYCurrent, focusAngleYTarget,
                ref _focusAngleYSmoothSpeed, focusAngleYSmoothTime);
            _focusDistanceCurrent = Mathf.SmoothDamp(_focusDistanceCurrent, focusDistanceTarget,
                ref _focusDistanceSmoothSpeed, 0.1f);


            // 计算相机姿态
            if (focusTarget)
            {
                _cameraRotation = Quaternion.Euler(_focusAngleXCurrent, _focusAngleYCurrent, 0);
                _cameraPosition = focusTarget.position - _cameraRotation * Vector3.forward * _focusDistanceCurrent +
                                  focusOffset;
            }
            else
            {
                _cameraRotation = Quaternion.Euler(_focusAngleXCurrent, _focusAngleYCurrent, 0);
                _cameraPosition = transform.position -
                                  (keyboardYDelta * focusOffsetMoveSensitive * forward +
                                   keyboardXDelta * focusOffsetMoveSensitive * right +
                                   keyboardZDelta * focusOffsetMoveSensitive * up);
            }


            // 更新相机姿态
            transform.rotation = _cameraRotation;
            transform.position = _cameraPosition;
        }

        private void UpdatePlayCamera()
        {
            if (!focusTarget)
            {
                return;
            }

            // 鼠标输入
            float scrollDelta = Mouse.current.scroll.ReadValue().y * focusZoomSensitive * 0.3f;
            float mouseXDelta = Mouse.current.delta.x.ReadValue() * focusAngleXSensitive * 0.3f;
            float mouseYDelta = Mouse.current.delta.y.ReadValue() * focusAngleYSensitive * 0.3f;

            focusOffset = Vector3.zero;

            focusAngleXTarget = Mathf.Clamp(focusAngleXTarget - mouseYDelta, focusAngleXMin, focusAngleXMax);
            focusAngleYTarget = (focusAngleYTarget + mouseXDelta) % 360;
            focusDistanceTarget =
                Mathf.Clamp(focusDistanceTarget - scrollDelta, focusDistanceMin, focusDistanceMax);


            // 平滑变换
            _focusAngleXCurrent = Mathf.SmoothDampAngle(_focusAngleXCurrent, focusAngleXTarget,
                ref _focusAngleXSmoothSpeed, focusAngleXSmoothTime);
            _focusAngleYCurrent = Mathf.SmoothDampAngle(_focusAngleYCurrent, focusAngleYTarget,
                ref _focusAngleYSmoothSpeed, focusAngleYSmoothTime);
            _focusDistanceCurrent = Mathf.SmoothDamp(_focusDistanceCurrent, focusDistanceTarget,
                ref _focusDistanceSmoothSpeed, 0.1f);


            // 计算相机姿态
            _cameraRotation = Quaternion.Euler(_focusAngleXCurrent, _focusAngleYCurrent, 0);
            _cameraPosition = focusTarget.position - _cameraRotation * Vector3.forward * _focusDistanceCurrent;


            // 更新相机姿态
            transform.rotation = _cameraRotation;
            transform.position = _cameraPosition;
        }
    }
}