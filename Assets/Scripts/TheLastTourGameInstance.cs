using System;
using System.Collections.Generic;
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

            RegisterManager<IGameManager>(new GameManager());
        }
    }


    public class TheLastTourGameInstance : MonoBehaviour
    {
        private static TheLastTourGameInstance _instance;

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
        }
    }
}