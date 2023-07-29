using System.Collections;
using System.Collections.Generic;
using TheLastTour.Controller.Machine;
using TheLastTour.Utility;
using UnityEngine;

namespace TheLastTour.Manager
{
    public interface IPartManager : IManager
    {
        public void RegisterPart(PartController part);

        public PartController GetPartPrefabByName(string partName);

        public PartController CreatePart(string partName);
        
        public PartController GetPartById(int id);
    }


    public class PartManager : IPartManager
    {
        Dictionary<string, PartController> partDict = new Dictionary<string, PartController>();

        public void Init(IArchitecture architecture)
        {
            Debug.Log("PartManager Init");
        }

        public void RegisterPart(PartController part)
        {
            partDict.Add(part.partName, part);
        }

        public PartController GetPartPrefabByName(string partName)
        {
            if (partDict.ContainsKey(partName))
            {
                return partDict[partName];
            }

            return null;
        }

        public PartController CreatePart(string partName)
        {
            if (partDict.ContainsKey(partName))
            {
                return GameObject.Instantiate(partDict[partName]);
            }

            return null;
        }

        public PartController GetPartById(int id)
        {
            return null;
        }
    }
}