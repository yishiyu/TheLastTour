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

        public PartController GetPartById(long id);

        public PartController CreatePart(string partName, long partId = -1);

        public void DestroyPart(PartController part);

        public void ClearAllParts();

        public void LoadExistingParts(List<PartController> parts);
    }


    public class PartManager : IPartManager
    {
        Dictionary<string, PartController> partPrefabDict = new Dictionary<string, PartController>();
        Dictionary<long, PartController> partInstanceDict = new Dictionary<long, PartController>();

        private int _uniqueSuffix = 0;

        private long GenerateUniqueId()
        {
            // 使用时间生成唯一ID,防止同一秒生成多个ID
            var date = System.DateTime.Now;
            var id = long.Parse(date.ToString("yyyyMMddHHmmss")) * 100 + _uniqueSuffix;
            _uniqueSuffix = (_uniqueSuffix + 1) % 100;

            return id;
        }

        public void Init(IArchitecture architecture)
        {
            Debug.Log("PartManager Init");
        }

        public void RegisterPart(PartController part)
        {
            partPrefabDict.Add(part.partName, part);
        }

        public PartController GetPartPrefabByName(string partName)
        {
            if (partPrefabDict.ContainsKey(partName))
            {
                return partPrefabDict[partName];
            }

            return null;
        }

        public PartController GetPartById(long id)
        {
            if (partInstanceDict.ContainsKey(id))
            {
                return partInstanceDict[id];
            }

            return null;
        }

        public PartController CreatePart(string partName, long partId = -1)
        {
            if (partPrefabDict.ContainsKey(partName))
            {
                PartController part = GameObject.Instantiate(partPrefabDict[partName]);
                if (partId < 0 || partInstanceDict.ContainsKey(partId))
                {
                    part.partId = GenerateUniqueId();
                }
                else
                {
                    part.partId = partId;
                }

                partInstanceDict.Add(part.partId, part);

                return part;
            }

            return null;
        }

        public void DestroyPart(PartController part)
        {
            if (part)
            {
                if (partInstanceDict.ContainsKey(part.partId))
                {
                    partInstanceDict.Remove(part.partId);
                }

                GameObject.Destroy(part.gameObject);
            }
        }

        private void InternalClearAllParts()
        {
            foreach (var part in partInstanceDict.Values)
            {
                if (part)
                {
                    GameObject.Destroy(part.gameObject);
                }
            }

            partInstanceDict.Clear();

            TheLastTourArchitecture.Instance.GetManager<IMachineManager>().ClearAllMachines();
        }

        public void ClearAllParts()
        {
            // 防止 PartManager 和 MachineManager 互相调用,导致死循环
            if (partInstanceDict.Count > 0)
            {
                InternalClearAllParts();
            }
        }

        public void LoadExistingParts(List<PartController> parts)
        {
            foreach (var part in parts)
            {
                partInstanceDict.Add(part.partId, part);
            }
        }
    }
}