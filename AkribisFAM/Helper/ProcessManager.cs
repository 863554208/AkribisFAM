using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AkribisFAM.Helper
{
    public class ProcessManager
    {
        public static void TerminateBackgroundProcess(string processName)
        {
            try
            {
                // Get all processes with the specified name
                Process[] processes = Process.GetProcessesByName(processName);

                if (processes.Length == 0)
                {
                    Console.WriteLine($"No process named '{processName}' is running.");
                    return;
                }

                foreach (Process proc in processes)
                {
                    // Avoid killing critical system processes
                    if (!proc.HasExited && proc.SessionId != 0)
                    {
                        proc.Kill();
                        proc.WaitForExit();
                        Console.WriteLine($"Terminated process: {proc.ProcessName} (ID: {proc.Id})");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error terminating process: {ex.Message}");
            }
        }
    }
}
