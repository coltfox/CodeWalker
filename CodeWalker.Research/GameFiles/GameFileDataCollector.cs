using CodeWalker.GameFiles;
using System.Diagnostics;
using System;
using System.IO;
using System.Collections.Generic;
using CodeWalker.Properties;

namespace CodeWalker.Research
{
    public abstract class GameFileDataCollector
    {
        public GameFileCache GameFileCache { get; } = GameFileCacheFactory.Create();
        public readonly string LogFilePath = "./logs";
        public abstract string TestName { get; }

        public double Progress;
        public string Status;

        public void Start()
        {
            InitCache();
            StartTraceLogFile();

            OnStart();

            foreach (RpfEntry entry in IterAllRpfEntries())
            {
                if (!IsValidEntry(entry))
                {
                    continue;
                }

                HandleEntry(entry);
            }

            OnEnd();
            GameFileCache.Clear();
        }

        public void StartTraceLogFile()
        {
            string timeStamp = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds.ToString();

            if (!Directory.Exists(LogFilePath))
            {
                Directory.CreateDirectory(LogFilePath);
            }

            string logFileName = string.Format("{0}/{1}_{2}.log", LogFilePath, TestName, timeStamp);

            File.WriteAllText(logFileName, string.Empty);

            Trace.Listeners.Add(new TextWriterTraceListener(logFileName));
            Trace.AutoFlush = true;
        }
        public IEnumerable<RpfEntry> IterAllRpfEntries()
        {
            GameFileCache.Init(UpdateStatus, UpdateStatus);
            List<RpfFile> allRpfs = GameFileCache.AllRpfs;

            for (int i = 0; i < allRpfs.Count; i++)
            {
                RpfFile file = allRpfs[i];
                double percentRpfs = (i / (float)allRpfs.Count) * 100;
                for (int j = 0; j < file.AllEntries.Count; j++)
                {
                    RpfEntry entry = file.AllEntries[j];
                    string entryName = Path.GetFileName(entry.Path);

                    Progress = (j / (float)file.AllEntries.Count) * 100;
                    UpdateStatus(string.Format("Testing {0} ({1:0.00}%) in {2} ({3:0.00}%))", entryName, Progress, file.Name, percentRpfs));

                    yield return entry;
                }
            }
        }

        private void InitCache()
        {
            UpdateStatus("Scanning...");

            if (!GTAFolder.IsCurrentGTAFolderValid())
            {
                Console.WriteLine(string.Format("Invalid GTA folder path '{0}'", GTAFolder.CurrentGTAFolder));
                GTAFolder.UpdateGTAFolder();
            }

            GTA5Keys.LoadFromPath(GTAFolder.CurrentGTAFolder, Settings.Default.Key);

            GameFileCache.EnableDlc = true;
            GameFileCache.Init(UpdateStatus, UpdateStatus);
        }

        public void UpdateStatus(string status)
        {
            Console.Clear();
            Console.Write(string.Format("\r{0}", status));
        }

        public virtual void OnStart() { }
        public virtual void OnEnd() { }

        public abstract bool IsValidEntry(RpfEntry entry);

        public abstract void HandleEntry(RpfEntry entry);
    }
}
