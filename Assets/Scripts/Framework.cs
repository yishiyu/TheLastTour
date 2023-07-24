using System.Collections.Generic;
using NotImplementedException = System.NotImplementedException;
using System;
using UnityEngine.Assertions;

namespace TheLastTour
{
    #region Architecture

    public interface IArchitecture
    {
        void RegisterManager<T>(T system) where T : IManager;
        void RegisterModel<T>(T model) where T : IModel;
        void RegisterUtility<T>(T utility) where T : IUtility;

        // 约束 T 是不为空的引用类型
        T GetManager<T>() where T : class, IManager;
        T GetModel<T>() where T : class, IModel;
        T GetUtility<T>() where T : class, IUtility;

        void ClearCommandStack();
        bool ExecuteCommand<T>(T command) where T : ICommand;
        bool ExecuteUndo();
        bool ExecuteRedo();
    }

    public abstract class Architecture<T> : IArchitecture where T : Architecture<T>, new()
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    CreateArchitecture();
                }

                return _instance;
            }
        }

        // 控制反转容器
        private IOCContainer _architectureContainer = new IOCContainer();


        // 重写该函数,注册具体的架构组件
        protected abstract void Init();


        public static void CreateArchitecture()
        {
            if (_instance == null)
            {
                // 创建并注册具体架构组件
                _instance = new T();
                _instance.Init();
            }
        }

        public void RegisterManager<TManager>(TManager system) where TManager : IManager
        {
            system.Init(this);
            _architectureContainer.Register<TManager>(system);
        }

        public void RegisterModel<TModel>(TModel model) where TModel : IModel
        {
            model.Init(this);
            _architectureContainer.Register<TModel>(model);
        }

        public void RegisterUtility<TUtility>(TUtility utility) where TUtility : IUtility
        {
            utility.Init(this);
            _architectureContainer.Register<TUtility>(utility);
        }

        public TManager GetManager<TManager>() where TManager : class, IManager
        {
            return _architectureContainer.Get<TManager>();
        }

        public TModel GetModel<TModel>() where TModel : class, IModel
        {
            return _architectureContainer.Get<TModel>();
        }

        public TUtility GetUtility<TUtility>() where TUtility : class, IUtility
        {
            return _architectureContainer.Get<TUtility>();
        }

        public void ClearCommandStack()
        {
            _commandUndoStack.Clear();
            _commandRedoStack.Clear();
        }


        // 有最大长度限制的命令栈
        // 到达长度上限后自动移除最早的命令
        // 限定内容为引用类型,栈空时 Pop 返回 null
        class CommandStack<TStackItem> where TStackItem : class
        {
            // private List<TStackItem> _commandList;
            TStackItem[] _commandList;
            private int _maxSize;
            private int _firstCommandIndex;
            private int _firstEmptyIndex;

            // 两个索引相遇时,队列为空

            private int FirstCommandIndex
            {
                get { return _firstCommandIndex; }
                set { _firstCommandIndex = (value + _maxSize) % _maxSize; }
            }

            private int FirstEmptyIndex
            {
                get { return _firstEmptyIndex; }
                set { _firstEmptyIndex = (value + _maxSize) % _maxSize; }
            }

            public int Count
            {
                get { return (_firstEmptyIndex - _firstCommandIndex + _maxSize) % _maxSize; }
            }

            public CommandStack(int maxSize)
            {
                Assert.IsTrue(maxSize > 1);

                _commandList = new TStackItem[maxSize + 1];
                _maxSize = maxSize + 1;
                _firstCommandIndex = 0;
                _firstEmptyIndex = 0;
            }

            public bool Push(TStackItem command)
            {
                _commandList[FirstEmptyIndex] = command;

                FirstEmptyIndex++;

                // 达到长度上限,需要移除最早的命令
                if (FirstEmptyIndex == FirstCommandIndex)
                {
                    FirstCommandIndex++;
                }

                return true;
            }

            public TStackItem Pop()
            {
                // 队列为空
                if (FirstCommandIndex == FirstEmptyIndex)
                {
                    return null;
                }

                FirstEmptyIndex--;
                return _commandList[FirstEmptyIndex];
            }

            public void Clear()
            {
                FirstCommandIndex = 0;
                FirstEmptyIndex = 0;
            }
        }

        private const int MAX_UNDO_COUNT = 100;
        private CommandStack<ICommand> _commandUndoStack = new CommandStack<ICommand>(MAX_UNDO_COUNT);
        private CommandStack<ICommand> _commandRedoStack = new CommandStack<ICommand>(MAX_UNDO_COUNT);

        public bool ExecuteCommand<TCommand>(TCommand command) where TCommand : ICommand
        {
            if (command.CanExecute())
            {
                bool result = command.Execute();

                // 仅有正确执行同时可以撤销的命令才会被加入撤销栈
                if (result && command.CanUndo())
                {
                    _commandUndoStack.Push(command);
                }

                // 执行命令后清空重做栈
                if (_commandRedoStack.Count > 0)
                {
                    _commandRedoStack.Clear();
                }


                return result;
            }

            return false;
        }

        public bool ExecuteUndo()
        {
            // 存在可以撤销的命令
            if (_commandUndoStack.Count > 0)
            {
                ICommand command = _commandUndoStack.Pop();
                if (command.CanUndo())
                {
                    bool result = command.Undo();
                    if (result)
                    {
                        _commandRedoStack.Push(command);
                    }

                    return result;
                }
            }

            return false;
        }

        public bool ExecuteRedo()
        {
            // 存在可以重做的命令
            if (_commandRedoStack.Count > 0)
            {
                ICommand command = _commandRedoStack.Pop();
                if (command.CanExecute())
                {
                    bool result = command.Execute();
                    if (result)
                    {
                        _commandUndoStack.Push(command);
                    }

                    return result;
                }
            }

            return false;
        }
    }


    public class IOCContainer
    {
        private Dictionary<Type, object> _container = new Dictionary<Type, object>();

        public void Register<T>(T obj)
        {
            var type = typeof(T);
            if (_container.ContainsKey(type))
            {
                _container[type] = obj;
            }
            else
            {
                _container.Add(type, obj);
            }
        }

        public T Get<T>() where T : class
        {
            var type = typeof(T);
            if (_container.ContainsKey(type))
            {
                return _container[type] as T;
            }
            else
            {
                return null;
            }
        }
    }


    public interface IManager
    {
        public void Init(IArchitecture architecture);
    }


    public interface IModel
    {
        public void Init(IArchitecture architecture);
    }


    public interface IUtility
    {
        public void Init(IArchitecture architecture);
    }

    #endregion

    #region Communication

    public interface ICommand
    {
        bool Execute()
        {
            throw new NotImplementedException();
        }

        bool CanExecute()
        {
            return true;
        }

        bool Undo()
        {
            throw new NotImplementedException();
        }

        bool CanUndo()
        {
            // 默认为不可撤销/重做
            return false;
        }
    }

    public interface IQuery<TResult>
    {
        TResult Query();
    }

    #endregion


    #region EventSystem

    // 可以使用静态的 GameEvent 对象来避免 GC
    public class GameEvent
    {
    }

    // 参考自 Unity 官方项目 Micro FPS
    // 整个架构的最底层,用于底层模块触发上层模块功能
    // usage:
    // 
    // EventBus.AddListener<CameraFocusOnTargetEvent>(OnCameraFocusOnTarget);
    // EventBus.RemoveListener<CameraFocusOnTargetEvent>(OnCameraFocusOnTarget);
    //
    // var CameraFocusOnTargetEvent = new CameraFocusOnTargetEvent();
    // CameraFocusOnTargetEvent.Transform = target.transform;
    // EventBus.Invoke(CameraFocusOnTargetEvent);
    public static class EventBus
    {
        // action -> wrapper action
        private static Dictionary<Delegate, Action<GameEvent>> _actionLookups =
            new Dictionary<Delegate, Action<GameEvent>>();

        // event type -> wrapper actions
        private static Dictionary<Type, Action<GameEvent>> _eventLookups = new Dictionary<Type, Action<GameEvent>>();

        public static void AddListener<T>(Action<T> action) where T : GameEvent
        {
            if (_actionLookups.ContainsKey(action))
            {
                return;
            }

            Action<GameEvent> wrapper = (e) => action((T)e);
            _actionLookups.Add(action, wrapper);

            if (!_eventLookups.ContainsKey(typeof(T)))
            {
                // avoid to invoke a null delegate
                // because the delegate may be removed
                _eventLookups.Add(typeof(T), (e) => { });
            }

            _eventLookups[typeof(T)] += wrapper;
        }

        public static void RemoveListener<T>(Action<T> action) where T : GameEvent
        {
            if (!_actionLookups.ContainsKey(action))
            {
                return;
            }

            Action<GameEvent> wrapper = _actionLookups[action];
            _actionLookups.Remove(action);

            _eventLookups[typeof(T)] -= wrapper;
        }

        public static void Invoke(GameEvent evt)
        {
            if (!_eventLookups.ContainsKey(evt.GetType()))
            {
                return;
            }

            _eventLookups[evt.GetType()]?.Invoke(evt);
        }

        public static void Clear()
        {
            _eventLookups.Clear();
            _actionLookups.Clear();
        }
    }

    #endregion
}