using System;
using System.Collections.Generic;
using TheLastTour.Controller.Machine;
using TheLastTour.Manager;
using TheLastTour.Utility;
using UnityEngine;


namespace TheLastTour
{
    public class TheLastTourArchitecture : Architecture<TheLastTourArchitecture>
    {
        protected override void Init()
        {
            Debug.Log("TheLastTourArchitecture Init");

            RegisterUtility<IInputUtility>(new InputUtility());

            RegisterManager<IGameStateManager>(new GameStateManager());
            RegisterManager<IMachineManager>(new MachineManager());
        }
    }


    public class TheLastTourGameInstance : MonoBehaviour
    {
        private static TheLastTourGameInstance _instance;

        public MachineController DefaultMachinePrefab;

        public static TheLastTourGameInstance Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<TheLastTourGameInstance>();
                }

                return _instance;
            }
        }


        private void Awake()
        {
            DontDestroyOnLoad(this);

            Debug.Log("TheLastTourGameInstance Start");

            TheLastTourArchitecture.CreateArchitecture();

            TheLastTourArchitecture.Instance.GetManager<IMachineManager>()
                .SetDefaultMachinePrefab(DefaultMachinePrefab);
        }
    }
}