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
            _uniqueSuffix++;

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
                    part.PartId = GenerateUniqueId();
                }
                else
                {
                    part.PartId = partId;
                }
                partInstanceDict.Add(part.PartId, part);

                return part;
            }

            return null;
        }

        public void DestroyPart(PartController part)
        {
            if (partInstanceDict.ContainsKey(part.PartId))
            {
                partInstanceDict.Remove(part.PartId);
            }

            GameObject.Destroy(part.gameObject);
        }
    }
}