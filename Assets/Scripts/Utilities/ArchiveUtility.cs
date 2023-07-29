using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using UnityEngine;

namespace TheLastTour.Utility
{
    public interface IArchiveUtility : IUtility
    {
        public bool SaveToArchive(string archiveType, string archiveName, string archiveContent);

        public bool LoadFromArchive(string archiveType, string archiveName, out string archiveContent);
    }

    public class DirectoryManager
    {
        private readonly DirectoryInfo _archiveRootDirectoryInfo = new DirectoryInfo("Archives");

        private readonly Dictionary<string, DirectoryInfo> _archiveDirectoryDict =
            new Dictionary<string, DirectoryInfo>();


        public DirectoryInfo GetDirectory(string archiveName)
        {
            if (!_archiveDirectoryDict.ContainsKey(archiveName))
            {
                _archiveDirectoryDict.Add(archiveName, _archiveRootDirectoryInfo.CreateSubdirectory(archiveName));
            }

            return _archiveDirectoryDict[archiveName];
        }
    }

    public class ArchiveUtility : IArchiveUtility
    {
        readonly DirectoryManager _directoryManager = new DirectoryManager();


        public void Init(IArchitecture architecture)
        {
            Debug.Log("ArchiveUtility Init");
        }


        public bool SaveToArchive(string archiveType, string archiveName, string archiveContent)
        {
            // 创建文件读写流
            string path = Path.Combine(_directoryManager.GetDirectory(archiveType).FullName, archiveName + ".json");

            {
                FileStream fileStream = new FileStream(path, File.Exists(path) ? FileMode.Truncate : FileMode.Create);
                StreamWriter fileWriter = new StreamWriter(fileStream);

                fileWriter.Write(archiveContent);
                fileWriter.Close();
                fileStream.Close();
            }

            return true;
        }

        public bool LoadFromArchive(string archiveType, string archiveName, out string archiveContent)
        {
            string path = Path.Combine(_directoryManager.GetDirectory(archiveType).FullName, archiveName + ".json");
            if (!File.Exists(path))
            {
                archiveContent = null;
                Debug.LogError("ArchiveUtility: LoadFromArchive: File not found: " + path);
                return false;
            }


            {
                FileStream fileStream = new FileStream(path, FileMode.Open);
                StreamReader fileReader = new StreamReader(fileStream);
                archiveContent = fileReader.ReadToEnd();

                fileReader.Close();
                fileStream.Close();
            }

            return true;
        }
    }
}