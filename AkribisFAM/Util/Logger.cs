using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AkribisFAM.Util
{
    public static class Logger
    {
        public static readonly BlockingCollection<string> _logQueue = new BlockingCollection<string>(new ConcurrentQueue<string>());
        //private static readonly string _baseDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        //private const long MaxLogFileSizeBytes = 5 * 1024 * 1024; // 5MB
        private static readonly string _baseDirectory = @" D:\Users\qiuxg\Desktop\新建文件夹(3)";//日志储存位置
        private const long MaxLogFileSizeBytes = 5 * 1024 * 1024; // 5MB大于5MB将备份日志文件
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

            // 注册程序退出事件，确保日志在退出前被完整写入
            AppDomain.CurrentDomain.ProcessExit += (s, e) => Shutdown();

            // 注册未处理异常事件，记录异常并确保日志写入
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                WriteLog("未处理异常: " + e.ExceptionObject.ToString());
                Shutdown();
            };

            // 捕获 Task 未处理的异常
            TaskScheduler.UnobservedTaskException += (sender, e) =>
            {
                WriteLog("任务未处理异常: " + e.Exception.Flatten().ToString());
                e.SetObserved(); // 标记异常已处理
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
            string logEntry = $"Thread:{threadId}  {timeStamp}  {Path.GetFileName(filePath)}: {lineNumber} - {message}";
            _logQueue.Add(logEntry);
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
                                        writer.WriteLine(logEntry);
                                    }
                                }
                                success = true; // 如果成功写入，退出重试循环
                            }
                            catch (Exception ex)
                            {
                                retries--; // 重试次数减1
                                if (retries == 0)
                                {
                                    // 如果三次都失败，记录错误日志
                                    //Console.Error.WriteLine($"写入日志失败，已重试三次: {ex.Message}");
                                }

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //Console.Error.WriteLine("日志线程异常: " + ex.Message);
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




