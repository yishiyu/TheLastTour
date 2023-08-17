using System.Collections;
using System.Collections.Generic;
using TheLastTour.Controller.Machine;
using TheLastTour.Manager;
using UnityEngine;

namespace TheLastTour.Command
{
    public class AddPartCommand : ICommand
    {
        private string _partName;

        private long _partId = -1;
        private int _rootJointId;
        private float _rotateAngleZ;

        private long _targetPartId;
        private int _targetJointId;

        private IPartManager _partManager;

        public AddPartCommand(
            string partName,
            int rootJointId,
            float rotateAngleZ,
            long targetPartId,
            int targetJointId)
        {
            _partName = partName;
            _rootJointId = rootJointId;
            _rotateAngleZ = rotateAngleZ;
            _targetPartId = targetPartId;
            _targetJointId = targetJointId;

            _partManager = TheLastTourArchitecture.Instance.GetManager<IPartManager>();
        }


        public bool Execute()
        {
            // 创建零件并保存 ID
            PartController part = _partManager.CreatePart(_partName, _partId);
            _partId = part.PartId;

            // 设置零件的位置和旋转
            part.SetRootJoint(_rootJointId);
            part.RotateAngleZ = _rotateAngleZ;

            // 获取目标 Joint
            PartController targetPart = _partManager.GetPartById(_targetPartId);
            PartJointController targetJoint = targetPart.GetJointById(_targetJointId);


            // 将零件连接到 joint, 禁止多重连接, 连接到 joint 所在机器
            part.Attach(targetJoint, true, true);
            part.partName = _partName;
            return true;
        }

        public bool Undo()
        {
            // 移除并销毁零件
            PartController part = _partManager.GetPartById(_partId);
            part.RemovePart(part, true);
            return true;
        }

        public bool CanExecute()
        {
            return true;
        }

        public bool CanUndo()
        {
            return true;
        }
    }
}