using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.IO;
using System.Configuration;
using System.Timers;

namespace K181292_Q3
{
    public partial class Service1 : ServiceBase
    {
        private static Timer timer = new Timer();
        private static Dictionary<string, DateTime> FileModificationTime = new Dictionary<string, DateTime>();
        private static List<string> UpdatedFiles = new List<string>();
        private static bool isFileUpdated = false;

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                string backupPath = ConfigurationManager.AppSettings["BackupDirectory"];
                string logPath = ConfigurationManager.AppSettings["LogsDirectory"];
                string trackingPath = ConfigurationManager.AppSettings["TrackingDirectory"];
                string[] fileNames = GetFileNames(trackingPath);

                if (!Directory.Exists(backupPath))
                {
                    Directory.CreateDirectory(backupPath);
                }
                if (!Directory.Exists(logPath))
                {
                    Directory.CreateDirectory(logPath);
                }

                GenerateLogs(DateTime.Now.ToString() + " FileWatcher Started!");

                timer.Interval = 60000; // 1 minute
                timer.Elapsed += new ElapsedEventHandler(TimerWrapper);
                timer.Enabled = true;
                timer.Start();

                // Initially making the backup
                foreach (string path in fileNames)
                {
                    string fileName = path.Split('\\')[path.Split('\\').Length - 1];

                    if (File.Exists(path))
                    {
                        File.Copy(path, Path.Combine(backupPath, fileName), true);
                    }

                    // Adding the Initial files Time with the key of fileName
                    DateTime modificationTime = File.GetLastWriteTime(path);
                    FileModificationTime.Add(fileName, modificationTime);

                    GenerateLogs(DateTime.Now.ToString() + " Initial Backup for " + fileName + " Completed");
                }

            }
            catch (Exception e)
            {
                GenerateLogs(e.Message);
            }
        }

        protected override void OnStop()
        {
            timer.Enabled = false;
            timer.Dispose();

            GenerateLogs(DateTime.Now.ToString() + " FileWatcher Stopped!");
        }

        private static void TimerWrapper(object sender, ElapsedEventArgs e)
        {
            Program();
        }

        private static void Program()
        {
            try
            {
                string trackingPath = ConfigurationManager.AppSettings["TrackingDirectory"];
                string backupPath = ConfigurationManager.AppSettings["BackupDirectory"];
                string[] fileNames = GetFileNames(trackingPath);

                CheckModification(fileNames);
                CopyFiles(trackingPath, backupPath);

                bool FileDeleted = isFileDeleted(fileNames);

                if (!(isFileUpdated || FileDeleted))
                {
                    GenerateLogs(DateTime.Now.ToString() + " No files are changed!");
                }

                if (timer.Interval == 3600000 && (isFileUpdated || FileDeleted)) // one hour condition
                {
                    timer.Interval = 60000;
                    GenerateLogs(DateTime.Now.ToString() + " Timer increased has reached the maximum of one hour!");
                }
                // if there is no change then increase the timer by two minute
                else if (timer.Interval != 3600000)
                {
                    if (!(isFileUpdated || FileDeleted))
                    {
                        Console.WriteLine("Timer increased");
                        timer.Interval += 120000;
                        GenerateLogs(DateTime.Now.ToString() + " Timer is increased by two minutes. The current interval is " + timer.Interval);
                    }
                }

                if (UpdatedFiles.Count > 0)
                {
                    // Refreshing the state variable
                    isFileUpdated = false;

                    // After copying the files - Flushing the UpdatedFiles
                    UpdatedFiles.Clear();
                }
            }
            catch (Exception e)
            {
                GenerateLogs(e.Message);
            }
        }
        private static string[] GetFileNames(string path)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    // This path is a directory
                    return Directory.GetFiles(path);
                }
                else
                    return null;
            }
            catch (Exception e)
            {
                GenerateLogs(e.Message);
                return null;
            }
        }

        private static void CheckModification(string[] fileNames)
        {
            try
            {
                foreach (string path in fileNames)
                {
                    DateTime modificationTime = File.GetLastWriteTime(path);
                    string fileName = path.Split('\\')[path.Split('\\').Length - 1];

                    if (FileModificationTime.ContainsKey(fileName))
                    {
                        if (modificationTime != FileModificationTime[fileName])
                        {
                            // Updating the modification time in the dictionary
                            FileModificationTime[fileName] = modificationTime;

                            // Keeping track of the path of updated files
                            UpdatedFiles.Add(fileName);

                            GenerateLogs(DateTime.Now.ToString() + " Existing file Changed");

                            isFileUpdated = true;
                        }
                    }
                    else
                    {
                        // Adding the Modification Time with the key of fileName
                        FileModificationTime.Add(fileName, modificationTime);
                        GenerateLogs(DateTime.Now.ToString() + " New file added");
                        // Keeping track of the path of newly added files
                        UpdatedFiles.Add(fileName);
                        isFileUpdated = true;
                    }
                }
            }
            catch (Exception e)
            {
                GenerateLogs(e.Message);
            }
        }

        private static void CopyFiles(string source, string destination)
        {
            try
            {
                foreach (string fileName in UpdatedFiles)
                {
                    if (File.Exists(Path.Combine(source, fileName)))
                    {
                        File.Copy(Path.Combine(source, fileName), Path.Combine(destination, fileName), true);
                    }

                    GenerateLogs(DateTime.Now.ToString() + " Backup for " + fileName + " Completed");
                }
            }
            catch (Exception e)
            {
                GenerateLogs(e.Message);
            }
        }

        private static bool isFileDeleted(string[] fileNames)
        {
            bool isFilesDeleted = false;
            List<string> deletedKeys = new List<string>();

            try
            {
                foreach (KeyValuePair<string, DateTime> entry in FileModificationTime)
                {
                    var fileExists = Array.Find(fileNames, element => element.Split('\\')[element.Split('\\').Length - 1] == entry.Key);
                    fileExists = String.IsNullOrEmpty(fileExists) == true ? " " : fileExists.Split('\\')[fileExists.Split('\\').Length - 1]; ;

                    if (entry.Key != fileExists)
                    {
                        GenerateLogs(DateTime.Now.ToString() + " File " + entry.Key + " is Deleted by the user!");
                        deletedKeys.Add(entry.Key);
                    }

                }

                foreach (string key in deletedKeys)
                {
                    // Deleting a key of a deleted file
                    FileModificationTime.Remove(key);
                    isFilesDeleted = true;
                }

                return isFilesDeleted;
            }
            catch (Exception e)
            {
                GenerateLogs(e.Message);
                return isFilesDeleted;
            }

        }

        public static void GenerateLogs(string log)
        {
            try
            {
                string path = ConfigurationManager.AppSettings["LogsDirectory"] + '\\' + "k180296_Q3_logs.txt";

                if (!File.Exists(path))
                {
                    using (StreamWriter writer = File.CreateText(path))
                    {
                        writer.WriteLine(log);
                    }
                }
                else
                {
                    using (StreamWriter writer = File.AppendText(path))
                    {
                        writer.WriteLine(log);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
