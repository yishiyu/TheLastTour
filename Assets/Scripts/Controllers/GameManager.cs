using System;
using System.Collections.Generic;
using System.Linq;
using TheLastTour.Controller.Machine;
using TheLastTour.Event;
using TheLastTour.Manager;
using TheLastTour.Utility;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace TheLastTour.Controller
{
    public class GameManager : MonoBehaviour
    {
        private IGameStateManager _gameStateManager;
        private InputActions _inputActions;

        // 机器零件预览预设
        public List<PartController> partPrefabs = new List<PartController>();
        private int _currentSelectedPartIndex = -1;

        public int CurrentSelectedPartIndex
        {
            get { return _currentSelectedPartIndex; }
            set
            {
                if (_currentSelectedPartIndex != value && value >= 0 && value < partPrefabs.Count)
                {
                    _currentSelectedPartIndex = value;
                    GameEvents.SelectedPartPrefabChangedEvent.CurrentSelectedPartIndex = value;
                    GameEvents.SelectedPartPrefabChangedEvent.CurrentSelectedPart = partPrefabs[value];
                    EventBus.Invoke(GameEvents.SelectedPartPrefabChangedEvent);
                }
            }
        }

        private PartController _partPreviewInstance;

        public MachineController machineController;

        private void Awake()
        {
            if (machineController == null)
            {
                machineController = FindObjectOfType<MachineController>();
            }
        }


        public void Start()
        {
            _gameStateManager = TheLastTourArchitecture.Instance.GetManager<IGameStateManager>();
            _inputActions = TheLastTourArchitecture.Instance.GetUtility<IInputUtility>().GetInputActions();
        }

        private void OnEnable()
        {
            EventBus.AddListener<GameStateChangedEvent>(OnGameStateChanged);
        }

        private void OnDisable()
        {
            EventBus.RemoveListener<GameStateChangedEvent>(OnGameStateChanged);
        }

        private void OnGameStateChanged(GameStateChangedEvent evt)
        {
            switch (evt.CurrentState)
            {
                case EGameState.Play:
                    // TODO 保存模型
                    machineController.TurnOnSimulation(true);
                    break;
                case EGameState.Edit:
                    // TODO 读取模型
                    // TODO 暂停模型物理模拟
                    break;
                default:
                    break;
            }
        }


        public void Update()
        {
            UpdateDebugAction();

            switch (_gameStateManager.GameState)
            {
                case EGameState.Edit:
                    UpdateEdit();
                    break;
                case EGameState.Play:

                    break;
                case EGameState.Pause:
                    break;
                case EGameState.GameOver:
                    break;
            }
        }

        private void UpdateDebugAction()
        {
            // Test Action
            if (Keyboard.current.numpad1Key.isPressed)
            {
                CurrentSelectedPartIndex = 0;
            }

            if (Keyboard.current.pKey.isPressed)
            {
                _gameStateManager.GameState = EGameState.Play;
            }
        }

        private void UpdateEdit()
        {
            if (machineController == null)
            {
                return;
            }

            // 操作 UI 时停止编辑
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                if (_partPreviewInstance != null)
                {
                    _partPreviewInstance.Detach();
                    _partPreviewInstance.gameObject.SetActive(false);
                }
            }

            switch (_gameStateManager.EditState)
            {
                case EEditState.Placing:

                    // 选择的零件非法
                    if (CurrentSelectedPartIndex < 0 || CurrentSelectedPartIndex >= partPrefabs.Count)
                    {
                        if (_partPreviewInstance != null)
                        {
                            _partPreviewInstance.Detach();
                            Destroy(_partPreviewInstance.gameObject);
                        }

                        return;
                    }

                    // 创建预览零件
                    if (_partPreviewInstance == null)
                    {
                        _partPreviewInstance = Instantiate(partPrefabs[CurrentSelectedPartIndex]);
                        _partPreviewInstance.gameObject.SetActive(false);
                    }


                    if (_partPreviewInstance != null)
                    {
                        // 预览零件跟随鼠标
                        if (PerformMouseTrace(out var position))
                        {
                            PartJointController joint = GetNearestJoint(position);

                            // 鼠标选中了其他零件, 预览零件未与其连接, 该零件也未连接到其他零件
                            if (joint != null && _partPreviewInstance.ConnectedJoint != joint &&
                                joint.IsAttached == false)
                            {
                                _partPreviewInstance.AttachTo(joint);
                                _partPreviewInstance.gameObject.SetActive(true);
                            }
                            // 选中的零件非法
                            else if (_partPreviewInstance != null)
                            {
                                _partPreviewInstance.Detach();
                                _partPreviewInstance.gameObject.SetActive(false);
                            }
                        }
                        // 鼠标没有选中其他零件, 暂时移除预览零件
                        else
                        {
                            _partPreviewInstance.Detach();
                            _partPreviewInstance.gameObject.SetActive(false);
                        }

                        // 点击左键, 将预览零件放置到场景中
                        if (Mouse.current.leftButton.wasPressedThisFrame &&
                            _partPreviewInstance.gameObject.activeInHierarchy)
                        {
                            PartController part = Instantiate(partPrefabs[CurrentSelectedPartIndex].gameObject,
                                machineController.transform).GetComponent<PartController>();

                            // 与预览零件连接的 joint
                            PartJointController joint = _partPreviewInstance.ConnectedJoint;

                            // 暂时移除预览零件
                            _partPreviewInstance.Detach();
                            _partPreviewInstance.gameObject.SetActive(false);

                            // 将零件连接到 joint
                            part.AttachTo(joint);
                            part.partName = partPrefabs[CurrentSelectedPartIndex].partName;

                            // 添加到机器中(计算质量,质心等)
                            machineController.AddPart(part);
                        }
                    }


                    break;

                default:
                    break;
            }
        }


        private bool PerformMouseTrace(out Vector3 position)
        {
            if (Camera.main)
            {
                Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
                if (Physics.Raycast(ray, out var hit, 1000f, -1, QueryTriggerInteraction.Ignore))
                {
                    position = hit.point;
                    return true;
                }
            }

            position = Vector3.zero;
            return false;
        }

        private List<PartJointController> GetAllJointInRadius(Vector3 center, float radius)
        {
            Collider[] colliders = Physics.OverlapSphere(center, radius, LayerMask.GetMask("PartJoint"),
                QueryTriggerInteraction.Ignore);

            return colliders.Select(collider => collider.GetComponent<PartJointController>()).ToList();
        }


        private PartJointController GetNearestJoint(Vector3 position)
        {
            PartJointController joint = null;
            foreach (PartJointController partJoint in GetAllJointInRadius(position, 5f))
            {
                if (joint == null || Vector3.Distance(position, partJoint.transform.position) <
                    Vector3.Distance(position, joint.transform.position))
                {
                    joint = partJoint;
                }
            }

            return joint;
        }
    }
}