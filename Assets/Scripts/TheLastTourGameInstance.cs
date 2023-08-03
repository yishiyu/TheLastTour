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
            RegisterUtility<IArchiveUtility>(new ArchiveUtility());

            RegisterManager<IGameStateManager>(new GameStateManager());
            RegisterManager<IMachineManager>(new MachineManager());
            RegisterManager<IPartManager>(new PartManager());
        }
    }


    public class TheLastTourGameInstance : MonoBehaviour
    {
        private static TheLastTourGameInstance _instance;

        public MachineController DefaultMachinePrefab;

        public List<PartController> partPrefabs = new List<PartController>();

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
            if (_instance == null)
            {
                _instance = this;
            }


            if (_instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                DontDestroyOnLoad(this);

                Debug.Log("TheLastTourGameInstance Start");

                TheLastTourArchitecture.CreateArchitecture();

                TheLastTourArchitecture.Instance.GetManager<IMachineManager>()
                    .SetDefaultMachinePrefab(DefaultMachinePrefab);

                IPartManager partManager = TheLastTourArchitecture.Instance.GetManager<IPartManager>();
                foreach (PartController part in partPrefabs)
                {
                    partManager.RegisterPart(part);
                }
            }
        }
    }
}