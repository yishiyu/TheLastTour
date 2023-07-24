using System.Collections;
using System.Collections.Generic;
using TheLastTour.Controller.Machine;
using Unity.VisualScripting;
using UnityEngine;

namespace TheLastTour.Manager
{
    // 每个分离的独立体都是一个单独的 Machine,其由 MachineManager 统一管理
    // 每个 Machine 管理一部分 Part, Machine 和 Machine 之间通过 Joint 进行连接
    // 两个 Machine 之间的 Joint 可以是刚性的,也可以是弹性的,也可以是合页连接
    // 当一个 Machine 中所有的 Part 都被摧毁时,该 Machine 就会被销毁
    // 当一个 Machine 中的某个 Part 被摧毁, 导致该 Machine 分离成两个独立体,则再创建一个新的 Machine 来管理分离出去的部分
    // 所有 Machine 中,只有一个核心组件无法摧毁,其所在的 Machine 为主 Machine


    public interface IMachineManager : IManager
    {
        public void TurnOnSimulation(bool isOn);

        public MachineController CreateEmptyMachine();

        public void SetDefaultMachinePrefab(MachineController machine);
    }

    public class MachineManager : IMachineManager
    {
        public void Init(IArchitecture architecture)
        {
            Debug.Log("MachineManager Init");

            MachineList.AddRange(Object.FindObjectsOfType<MachineController>());
        }


        // 管理 Machine 的列表
        public List<MachineController> MachineList = new List<MachineController>();
        private MachineController _defaultMachinePrefab;


        public void SetDefaultMachinePrefab(MachineController machine)
        {
            _defaultMachinePrefab = machine;
        }

        public MachineController CreateEmptyMachine()
        {
            if (_defaultMachinePrefab)
            {
                MachineController machine =
                    GameObject.Instantiate(_defaultMachinePrefab, Vector3.zero, Quaternion.identity);
                machine.Init();
                MachineList.Add(machine);
                return machine;
            }

            return null;
        }

        public void TurnOnSimulation(bool isOn)
        {
            foreach (var machine in MachineList)
            {
                machine.TurnOnSimulation(isOn);
            }
        }
    }
}