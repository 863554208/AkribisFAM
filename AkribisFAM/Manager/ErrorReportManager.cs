using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AkribisFAM.Util;

namespace AkribisFAM.Manager
{
    public class ErrorReportManager
    {
        public static ConcurrentQueue<Exception> ErrorQueue = new ConcurrentQueue<Exception>();

        public static void Report(Exception ex)
        {
            var context = TaskContextManager.GetCurrentContext();  // 获取当前上下文
            var taskInfo = context != null ? $"[Task: {context.TaskId}, Station: {context.StationName}]" : "[No Context]";

            // 打印错误信息
            Console.WriteLine($"{taskInfo} [Error] {DateTime.Now}: {ex.Message}\n{ex.StackTrace}");

            // 将异常添加到队列
            ErrorQueue.Enqueue(ex);
        }
    }
}
