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
        #region Instance

        private static GameManager _instance;

        public static GameManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<GameManager>();
                }

                return _instance;
            }
        }

        #endregion

        #region Part Prefabs

        public List<PartController> partPrefabs = new List<PartController>();
        private int _currentSelectedPartIndex = -1;

        public int CurrentSelectedPartIndex
        {
            get { return _currentSelectedPartIndex; }
            set
            {
                if (_currentSelectedPartIndex != value)
                {
                    if (_currentSelectedPartIndex >= 0 && _currentSelectedPartIndex < partPrefabs.Count)
                    {
                        if (value >= 0 && value < partPrefabs.Count)
                        {
                            // 从合法下标到另一个合法下标
                            _currentSelectedPartIndex = value;

                            GameEvents.SelectedPartPrefabChangedEvent.CurrentSelectedPartIndex = value;
                            GameEvents.SelectedPartPrefabChangedEvent.CurrentSelectedPart = partPrefabs[value];
                            EventBus.Invoke(GameEvents.SelectedPartPrefabChangedEvent);
                        }
                        else
                        {
                            // 从合法下标到非法下标
                            _currentSelectedPartIndex = -1;
                            _gameStateManager.EditState = EEditState.Selecting;

                            GameEvents.SelectedPartPrefabChangedEvent.CurrentSelectedPartIndex = -1;
                            EventBus.Invoke(GameEvents.SelectedPartPrefabChangedEvent);
                        }

                        if (_partPreviewInstance != null)
                        {
                            _partPreviewInstance.Detach();
                            _partManager.DestroyPart(_partPreviewInstance);
                            _partPreviewInstance = null;
                        }
                    }
                    else
                    {
                        if (value >= 0 && value < partPrefabs.Count)
                        {
                            // 从非法下标到合法下标
                            _currentSelectedPartIndex = value;
                            _gameStateManager.EditState = EEditState.Placing;

                            GameEvents.SelectedPartPrefabChangedEvent.CurrentSelectedPartIndex = value;
                            GameEvents.SelectedPartPrefabChangedEvent.CurrentSelectedPart = partPrefabs[value];
                            EventBus.Invoke(GameEvents.SelectedPartPrefabChangedEvent);
                        }
                        else
                        {
                            // 从非法下标到非法下标
                            // pass
                        }
                    }


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

        #endregion

        #region Part Selection

        public PartController selectedPart = null;

        public PartController SelectedPart
        {
            get { return selectedPart; }
            set
            {
                if (selectedPart != value)
                {
                    GameEvents.SelectedPartChangedEvent.PreviousSelectedPart = selectedPart;
                    GameEvents.SelectedPartChangedEvent.CurrentSelectedPart = value;
                    EventBus.Invoke(GameEvents.SelectedPartChangedEvent);

                    selectedPart = value;
                }
            }
        }

        public PartController mouseOverPart = null;

        #endregion

        private IGameStateManager _gameStateManager;
        private InputActions _inputActions;
        private IMachineManager _machineManager;
        private IPartManager _partManager;

        #region Initialization

        public void Start()
        {
            _gameStateManager = TheLastTourArchitecture.Instance.GetManager<IGameStateManager>();
            _inputActions = TheLastTourArchitecture.Instance.GetUtility<IInputUtility>().GetInputActions();
            _machineManager = TheLastTourArchitecture.Instance.GetManager<IMachineManager>();
            _partManager = TheLastTourArchitecture.Instance.GetManager<IPartManager>();

            CurrentSelectedPartIndex = -1;
            _gameStateManager.GameState = EGameState.Edit;
            _gameStateManager.EditState = EEditState.Selecting;
        }

        #endregion

        #region Event Handlers

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

                    var corePart = _machineManager.GetCorePart();
                    if (corePart)
                    {
                        GameEvents.FocusOnTargetEvent.Target = _machineManager.GetCorePart().transform;
                        EventBus.Invoke(GameEvents.FocusOnTargetEvent);
                    }

                    Cursor.lockState = CursorLockMode.Locked;

                    break;
                case EGameState.Edit:
                    // TODO 读取模型
                    // TODO 暂停模型物理模拟
                    break;
                default:
                    break;
            }
        }

        #endregion


        public void Update()
        {
            UpdateDebugAction();

            switch (_gameStateManager.GameState)
            {
                case EGameState.Edit:
                    UpdateEdit();
                    break;
                case EGameState.Play:
                    UpdatePlay();
                    break;
                case EGameState.Pause:
                    break;
                case EGameState.GameOver:
                    break;
            }
        }

        private void UpdateDebugAction()
        {
            // 选择零件 空
            if (Keyboard.current.backquoteKey.wasPressedThisFrame)
            {
                CurrentSelectedPartIndex = -1;
            }

            // 修改零件根 joint
            if (Keyboard.current.fKey.wasPressedThisFrame)
            {
                if (_partPreviewInstance)
                {
                    _partPreviewInstance.IterRootJoint();
                    _partPreviewInstance.TurnOnJointCollision(false);
                }
            }

            if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                if (_partPreviewInstance)
                {
                    _partPreviewInstance.IterRotateAngleZ();
                    _partPreviewInstance.TurnOnJointCollision(false);
                }
            }


            // 测试保存和读取
            if (Keyboard.current.kKey.wasPressedThisFrame)
            {
                _machineManager.SaveMachines("test");
            }

            if (Keyboard.current.lKey.wasPressedThisFrame)
            {
                _machineManager.LoadMachines("test");
            }
        }

        private void UpdateEdit()
        {
            // 操作 UI 时停止编辑
            if (Mouse.current.leftButton.isPressed && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            bool isMouseHit = PerformMouseTrace(out var hit);

            switch (_gameStateManager.EditState)
            {
                case EEditState.Placing:

                    // 选择的零件非法
                    if (CurrentSelectedPartIndex < 0 || CurrentSelectedPartIndex >= partPrefabs.Count)
                    {
                        if (_partPreviewInstance != null)
                        {
                            _partPreviewInstance.Detach();
                            _partManager.DestroyPart(_partPreviewInstance);
                            _partPreviewInstance = null;
                            // Destroy(_partPreviewInstance.gameObject);
                        }

                        return;
                    }

                    // 创建预览零件
                    if (_partPreviewInstance == null)
                    {
                        // _partPreviewInstance = Instantiate(partPrefabs[CurrentSelectedPartIndex]);
                        _partPreviewInstance = _partManager.CreatePart(partPrefabs[CurrentSelectedPartIndex].name);
                        _partPreviewInstance.EditType = EEditType.PreviewDisable;
                    }

                    if (_partPreviewInstance != null)
                    {
                        // 预览零件跟随鼠标
                        if (isMouseHit)
                        {
                            PartJointController joint = _machineManager.GetNearestJoint(hit.point);

                            // 未连接
                            if (_partPreviewInstance.ConnectedJoint == null)
                            {
                                // 鼠标选中了其他零件, 预览零件未与其连接, 该零件也未连接到其他零件
                                if (joint != null && joint.IsAttached == false)
                                {
                                    _partPreviewInstance.Attach(joint, false);
                                    _partPreviewInstance.EditType = EEditType.PreviewEnable;
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
                                    }
                                }
                                // 未选中
                                else
                                {
                                    _partPreviewInstance.Detach();
                                    _partPreviewInstance.EditType = EEditType.PreviewDisable;
                                }
                            }
                        }
                        // 鼠标没有选中其他零件, 暂时移除预览零件
                        else
                        {
                            _partPreviewInstance.Detach();
                            _partPreviewInstance.EditType = EEditType.PreviewDisable;
                        }

                        // 点击左键, 将预览零件放置到场景中
                        if (Mouse.current.leftButton.wasPressedThisFrame &&
                            _partPreviewInstance.gameObject.activeInHierarchy)
                        {
                            // ISimulator simulator = _partPreviewInstance.ConnectedJoint.Owner;

                            // PartController part = Instantiate(partPrefabs[CurrentSelectedPartIndex].gameObject)
                            //     .GetComponent<PartController>();
                            PartController part = _partManager.CreatePart(partPrefabs[CurrentSelectedPartIndex].name);

                            // 与预览零件连接的 joint
                            PartJointController joint = _partPreviewInstance.ConnectedJoint;
                            part.SetRootJoint(_partPreviewInstance.RootJointId);
                            part.RotateAngleZ = _partPreviewInstance.RotateAngleZ;

                            // 暂时移除预览零件
                            _partPreviewInstance.Detach();
                            _partPreviewInstance.EditType = EEditType.PreviewDisable;

                            // 将零件连接到 joint, 禁止多重连接, 连接到 joint 所在机器
                            part.Attach(joint, true, true);
                            part.partName = partPrefabs[CurrentSelectedPartIndex].partName;
                        }
                    }


                    break;

                case EEditState.Selecting:
                    if (_partPreviewInstance != null)
                    {
                        _partPreviewInstance.Detach();
                        _partPreviewInstance.EditType = EEditType.PreviewDisable;
                    }


                    // 鼠标悬浮在物体上
                    if (isMouseHit)
                    {
                        // 上一帧鼠标悬浮物体不为空
                        if (mouseOverPart != null)
                        {
                            // 悬浮在新物体上
                            if (mouseOverPart.gameObject != hit.collider.transform.parent.gameObject)
                            {
                                // 清空上一个鼠标悬浮零件状态
                                mouseOverPart.EditType = EEditType.Default;
                                mouseOverPart = null;


                                if (selectedPart == null ||
                                    hit.collider.transform.parent.gameObject != selectedPart.gameObject)
                                {
                                    mouseOverPart = hit.collider.gameObject.GetComponentInParent<PartController>();
                                    mouseOverPart.EditType = EEditType.MouseOver;
                                }
                            }
                        }
                        // 上一帧鼠标悬浮物体为空
                        else
                        {
                            if (selectedPart == null ||
                                hit.collider.transform.parent.gameObject != selectedPart.gameObject)
                            {
                                mouseOverPart = hit.collider.gameObject.GetComponentInParent<PartController>();
                                if (mouseOverPart != null)
                                {
                                    mouseOverPart.EditType = EEditType.MouseOver;
                                }
                            }
                        }
                    }
                    // 鼠标没有悬浮在零件上
                    else if (mouseOverPart != null)
                    {
                        mouseOverPart.EditType = EEditType.Default;
                        mouseOverPart = null;
                    }


                    if (Mouse.current.leftButton.wasPressedThisFrame)
                    {
                        if (isMouseHit && mouseOverPart != null)
                        {
                            if (selectedPart != null)
                            {
                                SelectedPart.EditType = EEditType.Default;
                                SelectedPart = null;
                            }

                            SelectedPart = mouseOverPart;
                            mouseOverPart = null;
                            SelectedPart.EditType = EEditType.Selected;
                        }
                        else
                        {
                            if (selectedPart != null)
                            {
                                SelectedPart.EditType = EEditType.Default;
                                SelectedPart = null;
                            }
                        }
                    }

                    if (Keyboard.current.fKey.wasPressedThisFrame)
                    {
                        if (selectedPart != null)
                        {
                            GameEvents.FocusOnTargetEvent.Target = selectedPart.transform;
                            EventBus.Invoke(GameEvents.FocusOnTargetEvent);
                            Debug.Log("Focus On Target");
                        }
                    }

                    if (Keyboard.current.tabKey.wasPressedThisFrame)
                    {
                        if (selectedPart != null)
                        {
                            selectedPart.RemovePart(selectedPart);
                        }
                    }

                    break;
                default:
                    break;
            }
        }

        private void UpdatePlay()
        {
        }

        #region Utils

        private bool PerformMouseTrace(out RaycastHit hit, int layerMask = 1 << 6)
        {
            if (Camera.main)
            {
                Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
                if (Physics.Raycast(ray, out hit, 1000f, layerMask, QueryTriggerInteraction.Ignore))
                {
                    return true;
                }
            }

            hit = new RaycastHit();
            return false;
        }

        #endregion
    }
}