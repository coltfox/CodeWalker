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
        public RpfManager RpfMan;
        public readonly string LogFilePath = "./logs";
        public abstract string TestName { get; }

        public double Progress;
        public string Status;

        public GameFileDataCollector()
        {
            RpfMan = new RpfManager();
        }

        public void Start(int? numFilesToTest = null)
        {
            InitCache();
            StartTraceLogFile();

            OnStart();
            int numFilesTested = 0;

            foreach (RpfEntry entry in IterAllRpfEntries())
            {
                if (numFilesToTest != null && numFilesTested >= numFilesToTest)
                {
                    break;
                }

                if (!IsValidEntry(entry))
                {
                    continue;
                }

                numFilesTested++;

                try
                {
                    HandleEntry(entry);
                }
                catch (Exception e)
                {
                    string errorText = string.Format("Error encountered while testing '{0}'!\n{1}", Path.GetFileName(entry.Path), e.ToString());
                    Trace.WriteLine(errorText);
                    UpdateStatus(errorText);
                }
            }

            OnEnd(numFilesTested);
        }

        public void StartTraceLogFile()
        {
            string timeStamp = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds.ToString();

            if (!Directory.Exists(LogFilePath))
            {
                Directory.CreateDirectory(LogFilePath);
            }

            string logFileName = string.Format("{0}/{1}.{2}.log", LogFilePath, TestName, timeStamp);

            File.WriteAllText(logFileName, string.Empty);

            Trace.Listeners.Add(new TextWriterTraceListener(logFileName));
            Trace.AutoFlush = true;
        }
        public IEnumerable<RpfEntry> IterAllRpfEntries()
        {
            List<RpfFile> allRpfs = RpfMan.AllRpfs;

            for (int i = 0; i < allRpfs.Count; i++)
            {
                RpfFile file = allRpfs[i];
                double percentRpfs = (double) i / allRpfs.Count;
                for (int j = 0; j < file.AllEntries.Count; j++)
                {
                    RpfEntry entry = file.AllEntries[j];
                    string entryName = Path.GetFileName(entry.Path);

                    Progress = (double) j / file.AllEntries.Count;
                    UpdateStatus(string.Format("Testing ({0:P2}) in {1} ({2:P2})\n\t'{3}')", Progress, file.Name, percentRpfs, entryName));

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

            RpfMan.Init(GTAFolder.CurrentGTAFolder, UpdateStatus, UpdateStatus);
        }

        public void UpdateStatus(string status)
        {
            Console.Clear();
            Console.Write(string.Format("\r{0}", status));
        }

        public virtual void OnStart() { }
        public virtual void OnEnd(int numFilesTested)
        {
            Trace.WriteLine(string.Format("{0} files tested.", numFilesTested));
        }

        public abstract bool IsValidEntry(RpfEntry entry);

        public abstract void HandleEntry(RpfEntry entry);
    }
}
