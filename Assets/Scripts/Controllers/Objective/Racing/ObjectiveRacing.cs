using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheLastTour.Controller.Objective
{
    [RequireComponent(typeof(Collider))]
    public class ObjectiveRacing : Manager.Objective
    {
        public float timeLimit = 60f;
        public bool isStart = false;

        public List<RacingCheckPoint> checkPointList = new List<RacingCheckPoint>();
        public int arrivedCheckPointCount = 0;


        public void CheckPointArrived(RacingCheckPoint checkPoint)
        {
            checkPoint.DisableCheckPoint();

            arrivedCheckPointCount++;

            if (arrivedCheckPointCount == checkPointList.Count)
            {
                descriptionText = "任务完成!";
                CompleteObjective();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player") && !isStart)
            {
                isStart = true;
                descriptionText = "Racing Start!";
                arrivedCheckPointCount = 0;
                UpdateObjective(descriptionText);

                foreach (var checkPoint in checkPointList)
                {
                    checkPoint.EnableCheckPoint();
                }
            }
        }

        private void Update()
        {
            if (isStart)
            {
                timeLimit -= Time.deltaTime;
                if (timeLimit < 0)
                {
                    timeLimit = 0;
                    descriptionText = "时间耗尽!";
                    UpdateObjective(descriptionText);

                    foreach (var checkPoint in checkPointList)
                    {
                        checkPoint.DisableCheckPoint();
                    }

                    isStart = false;
                    return;
                }

                descriptionText = "时间剩余: " + timeLimit;
                UpdateObjective(descriptionText);
            }
        }
    }
}