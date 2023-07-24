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
        private IMachineManager _machineManager;

        // 机器零件预览预设
        public List<PartController> partPrefabs = new List<PartController>();
        private int _currentSelectedPartIndex = -1;

        public int CurrentSelectedPartIndex
        {
            get { return _currentSelectedPartIndex; }
            set
            {
                if (_currentSelectedPartIndex != value)
                {
                    _currentSelectedPartIndex = value;
                    GameEvents.SelectedPartPrefabChangedEvent.CurrentSelectedPartIndex = value;
                    if (value >= 0 && value < partPrefabs.Count)
                    {
                        GameEvents.SelectedPartPrefabChangedEvent.CurrentSelectedPart = partPrefabs[value];
                    }

                    EventBus.Invoke(GameEvents.SelectedPartPrefabChangedEvent);
                }
            }
        }

        private PartController _partPreviewInstance;

        public List<PartController> selectedParts = new List<PartController>();


        public void Start()
        {
            _gameStateManager = TheLastTourArchitecture.Instance.GetManager<IGameStateManager>();
            _inputActions = TheLastTourArchitecture.Instance.GetUtility<IInputUtility>().GetInputActions();
            _machineManager = TheLastTourArchitecture.Instance.GetManager<IMachineManager>();
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
                    TheLastTourArchitecture.Instance.GetManager<IMachineManager>().TurnOnSimulation(true);
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
            // 切换到放置模式
            if (Keyboard.current.numpad1Key.isPressed)
            {
                _gameStateManager.EditState = EEditState.Placing;
            }

            // 切换到选择模式
            if (Keyboard.current.numpad2Key.isPressed)
            {
                _gameStateManager.EditState = EEditState.Selecting;
            }

            // 选择零件 空
            if (Keyboard.current.numpad4Key.isPressed)
            {
                CurrentSelectedPartIndex = -1;
            }

            // 选择零件 1
            if (Keyboard.current.numpad5Key.isPressed)
            {
                CurrentSelectedPartIndex = 0;
            }

            // 开始游戏
            if (Keyboard.current.pKey.isPressed)
            {
                _gameStateManager.GameState = EGameState.Play;
            }
        }


        private void UpdateEdit()
        {
            // // 操作 UI 时停止编辑
            // // 这个判断有问题
            // if (!EventSystem.current.IsPointerOverGameObject())
            // {
            //     if (_partPreviewInstance != null)
            //     {
            //         _partPreviewInstance.Detach();
            //         _partPreviewInstance.gameObject.SetActive(false);
            //     }
            //
            //     Debug.Log("Mouse On UI");
            //     return;
            // }

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
                        _partPreviewInstance.TurnOnJointCollision(false);
                        _partPreviewInstance.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
                    }


                    if (_partPreviewInstance != null)
                    {
                        // 预览零件跟随鼠标
                        if (PerformMouseTrace(out var hit))
                        {
                            PartJointController joint = _machineManager.GetNearestJoint(hit.point);

                            // 未连接
                            if (_partPreviewInstance.ConnectedJoint == null)
                            {
                                // 鼠标选中了其他零件, 预览零件未与其连接, 该零件也未连接到其他零件
                                if (joint != null && joint.IsAttached == false)
                                {
                                    _partPreviewInstance.Attach(joint, false);
                                    _partPreviewInstance.gameObject.SetActive(true);
                                }
                            }
                            // 已连接
                            else
                            {
                                if (joint != null)
                                {
                                    // 鼠标选中了其他零件, 预览零件未与其连接, 该零件也未连接到其他零件
                                    if (_partPreviewInstance.ConnectedJoint != joint)
                                    {
                                        _partPreviewInstance.Detach();
                                        _partPreviewInstance.Attach(joint, false);
                                        _partPreviewInstance.gameObject.SetActive(true);
                                    }
                                }
                                // 未选中
                                else
                                {
                                    _partPreviewInstance.Detach();
                                    _partPreviewInstance.gameObject.SetActive(false);
                                }
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
                            MachineController machine = _partPreviewInstance.ConnectedJoint.Owner.GetOwnedMachine();

                            PartController part = Instantiate(partPrefabs[CurrentSelectedPartIndex].gameObject,
                                machine.transform).GetComponent<PartController>();

                            // 与预览零件连接的 joint
                            PartJointController joint = _partPreviewInstance.ConnectedJoint;

                            // 暂时移除预览零件
                            _partPreviewInstance.Detach();
                            _partPreviewInstance.gameObject.SetActive(false);

                            // 将零件连接到 joint
                            part.Attach(joint);
                            part.partName = partPrefabs[CurrentSelectedPartIndex].partName;

                            // 添加到机器中(计算质量,质心等)
                            machine.AddPart(part);
                        }
                    }


                    break;

                case EEditState.Selecting:
                    if (_partPreviewInstance != null)
                    {
                        _partPreviewInstance.Detach();
                        _partPreviewInstance.gameObject.SetActive(false);
                    }

                    if (Mouse.current.leftButton.wasPressedThisFrame)
                    {
                        // 非多选模式,清空已选内容
                        if (!Keyboard.current.leftCtrlKey.isPressed)
                        {
                            selectedParts.Clear();
                        }

                        if (PerformMouseTrace(out var hit))
                        {
                            PartController part = hit.collider.gameObject.GetComponent<PartController>();
                            if (part != null)
                            {
                                if (!selectedParts.Contains(part))
                                {
                                    selectedParts.Add(part);
                                }
                                else
                                {
                                    selectedParts.Remove(part);
                                }
                            }
                        }
                    }

                    if (Keyboard.current.fKey.wasPressedThisFrame)
                    {
                        if (selectedParts.Count > 0)
                        {
                            GameEvents.FocusOnTargetEvent.Target = selectedParts[0].transform;
                            EventBus.Invoke(GameEvents.FocusOnTargetEvent);
                            Debug.Log("Focus On Target");
                        }
                    }

                    if (Keyboard.current.deleteKey.wasPressedThisFrame)
                    {
                        foreach (var part in selectedParts)
                        {
                            MachineController machine = part.GetOwnedMachine();
                            machine.RemovePart(part);
                            // part.Detach();
                            // Destroy(part.gameObject);
                        }

                        selectedParts.Clear();
                    }

                    break;
                case EEditState.Deleting:
                    break;
                case EEditState.Setting:
                    break;
                default:
                    break;
            }
        }


        private bool PerformMouseTrace(out RaycastHit hit)
        {
            if (Camera.main)
            {
                Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
                if (Physics.Raycast(ray, out hit, 1000f, 1 << 6, QueryTriggerInteraction.Ignore))
                {
                    return true;
                }
            }

            hit = new RaycastHit();
            return false;
        }
    }
}