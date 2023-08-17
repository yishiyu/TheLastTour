using System.Collections;
using System.Collections.Generic;
using TheLastTour.Controller.Machine;
using TheLastTour.Manager;
using TheLastTour.Utility;
using UnityEngine;

namespace TheLastTour.Command
{
    public class RemovePartCommand : ICommand
    {
        private long partId;

        private string path;
        private string fileName;

        private IPartManager _partManager;
        private IMachineManager _machineManager;
        private IArchiveUtility _archiveUtility;


        public RemovePartCommand(
            PartController part)
        {
            partId = part.PartId;


            _partManager = TheLastTourArchitecture.Instance.GetManager<IPartManager>();
            _machineManager = TheLastTourArchitecture.Instance.GetManager<IMachineManager>();
            _archiveUtility = TheLastTourArchitecture.Instance.GetUtility<IArchiveUtility>();

            fileName = "EditArchive-" + partId;
            path = _archiveUtility.GetTempArchivePath();
        }


        public bool Execute()
        {
            // 存储存档
            _machineManager.SaveMachines(fileName, path);

            // 创建零件并保存 ID
            PartController part = _partManager.GetPartById(partId);

            part.RemovePart(part, true);
            return true;
        }

        public bool Undo()
        {
            // 恢复存档
            _machineManager.LoadMachines(fileName, path);

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