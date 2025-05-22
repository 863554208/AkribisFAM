using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AkribisFAM.Manager
{
    public static class TaskContextManager
    {
        private static AsyncLocal<TaskContext> _context = new AsyncLocal<TaskContext>();

        public static void SetContext(TaskContext context) => _context.Value = context;

        public static TaskContext GetCurrentContext() => _context.Value;

        public static void ClearContext() => _context.Value = null;
    }


    public class TaskContext
    {
        public string StationName { get; set; }
        public DateTime StartTime { get; set; }
        public string TaskId { get; set; }  // 可选，用来标识当前任务
    }

}
