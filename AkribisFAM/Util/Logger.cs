using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AkribisFAM.Util
{
    public static class Logger
    {
        public static readonly BlockingCollection<string> _logQueue = new BlockingCollection<string>(new ConcurrentQueue<string>());
        //private static readonly string _baseDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        //private const long MaxLogFileSizeBytes = 5 * 1024 * 1024; // 5MB
        private static readonly string _baseDirectory = @" D:\Users\qiuxg\Desktop\Log";//log path
        private const long MaxLogFileSizeBytes = 20 * 1024 * 1024; 
        private static readonly Thread _logThread;
        private static volatile bool _isRunning = true;
        private static readonly object _fileLock = new object();

        static Logger()
        {
            _logThread = new Thread(ProcessLogQueue)
            {
                IsBackground = true,
                Name = "LoggerThread"
            };
            _logThread.Start();

            AppDomain.CurrentDomain.ProcessExit += (s, e) => Shutdown();

            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                WriteLog("Unhandled Exception: " + e.ExceptionObject.ToString());
                Shutdown();
            };

            TaskScheduler.UnobservedTaskException += (sender, e) =>
            {
                WriteLog("Unobserved Task Exception: " + e.Exception.Flatten().ToString());
                e.SetObserved(); 
            };
        }

        private static string GetLogFilePath()
        {
            string date = DateTime.Now.ToString("yyyy-MM-dd");
            string folderPath = Path.Combine(_baseDirectory, date);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string baseFileName = $"{date}_log.txt";
            string logFilePath = Path.Combine(folderPath, baseFileName);
            lock (_fileLock)
            {
                if (File.Exists(logFilePath) && new FileInfo(logFilePath).Length > MaxLogFileSizeBytes)
                {
                    string backupFile = Path.Combine(folderPath, $"{date}_log_{DateTime.Now:HHmmss}_backup.txt");
                    File.Move(logFilePath, backupFile);
                }
            }
            return logFilePath;
        }

        public static void WriteLog(string message,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        {
            string timeStamp = DateTime.Now.ToString("yyyyMMdd_HH:mm:ss.fff");
            string threadId = Thread.CurrentThread.ManagedThreadId.ToString();
            //string logEntry = $"Thread:{threadId}  {timeStamp}  {Path.GetFileName(filePath)}: {lineNumber} - {message}";
            string logEntry = $"[{timeStamp}]: User: {GlobalManager.Current.username} Behavior: {message}";
            _logQueue.Add(logEntry);
        }

        public static Action<string> OnLog;

        public static void Log(string message)
        {
            OnLog?.Invoke(message);
        }

        private static void ProcessLogQueue()
        {
            try
            {
                while (_isRunning || !_logQueue.IsCompleted)
                {
                    if (_logQueue.TryTake(out var logEntry, Timeout.Infinite))
                    {
                        bool success = false;
                        int retries = 3;
                        while (retries > 0 && !success)
                        {
                            try
                            {
                                string logFilePath = GetLogFilePath();
                                lock (_fileLock)
                                {
                                    using (FileStream fs = new FileStream(logFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite, 4096, FileOptions.WriteThrough))
                                    using (StreamWriter writer = new StreamWriter(fs, Encoding.UTF8))
                                    {
                                        Log(logEntry);
                                        writer.WriteLine(logEntry);
                                    }
                                }
                                success = true;
                            }
                            catch (Exception ex)
                            {
                                retries--; 
                                if (retries == 0)
                                {
                                }

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        public static void Shutdown()
        {
            if (!_isRunning) return;
            _isRunning = false;
            _logQueue.CompleteAdding();
            _logThread.Join();
        }
    }
}




