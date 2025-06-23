using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading;
using System;
using System.Diagnostics;
using Newtonsoft.Json;

namespace AkribisFAM.ViewModel
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        protected System.Timers.Timer _timer;
        Thread tdUpdate;
        [Browsable(false), NotMapped, JsonIgnore]
        public bool thAlive { get; set; } = true;
        [Browsable(false), NotMapped, JsonIgnore]
        public bool thRun { get; set; } = true;
        [Browsable(false), NotMapped, JsonIgnore]
        public int thSleepTime { get; set; } = 100;

        // Get current process
        private Process currentProcess;

        // Measure initial memory and CPU time
        private long initialMemory;
        private long finalMemory;
        private TimeSpan initialCpuTime;
        private TimeSpan finalCpuTime;

        // Calculate resource consumption
        [Browsable(false), NotMapped, JsonIgnore]
        private long memoryUsed => finalMemory - initialMemory;
        [Browsable(false), NotMapped, JsonIgnore]
        private TimeSpan cpuUsed => finalCpuTime - initialCpuTime;
        [Browsable(false), NotMapped, JsonIgnore]
        public string MemoryUsed => $"{memoryUsed / 1024.0 / 1024.0:F2} MB";
        [Browsable(false), NotMapped, JsonIgnore]
        public string CPUTimeUsed => $"{cpuUsed.TotalMilliseconds:F2} ms";

        public void KillUpdateThread()
        {
            thAlive = false;
        }
        public void PauseUpdateThread()
        {
            thRun = false;
        }
        public void ResumeUpdateThread()
        {
            thRun = true;
        }
        public void SetThreadInterval(int threadSleepTime)
        {
            thSleepTime = threadSleepTime;
        }
        public ViewModelBase()
        {
            _timer = new System.Timers.Timer(1000);
            tdUpdate = new Thread(UpdateThread);
        }

        public void SetTimerInterval(int interval)
        {
            _timer = new System.Timers.Timer(interval);
        }

        public virtual void UpdateThread()
        {
            // Get current process
            currentProcess = Process.GetCurrentProcess();

            while (thAlive)
            {
                if (thRun)
                {
                    // Measure initial memory and CPU time
                    initialMemory = currentProcess.WorkingSet64;
                    initialCpuTime = currentProcess.TotalProcessorTime;

                    ThreadBody();

                    // Measure memory and CPU time after execution
                    finalMemory = currentProcess.WorkingSet64;
                    finalCpuTime = currentProcess.TotalProcessorTime;
                }
                Thread.Sleep(thSleepTime);
                //Thread.Sleep(10);

            }

        }

        public virtual void ThreadBody() { }
        public void SetUpdateThread(Thread thread)
        {
            if (thread == null) return;

            tdUpdate = thread;
        }
        public void StartUpdateThread()
        {
            if (tdUpdate == null) return;

            tdUpdate.Start();
        }
        public void IntializeUpdateThread(string threadName, int timeInterval)
        {
            tdUpdate.Name = threadName;
            SetThreadInterval(timeInterval);
            StartUpdateThread();
            PauseUpdateThread();

        }
        public void TerminateUpdateThread()
        {
            if (tdUpdate == null) return;
            KillUpdateThread();
            PauseUpdateThread();
            tdUpdate.Abort();
            tdUpdate = null;
        }
        ~ViewModelBase()
        {
            TerminateUpdateThread();
        }

        public class RelayCommand : ICommand
        {
            // Declare the delegate
            private readonly Action execute;

            // Constructor
            public RelayCommand(Action execute)
            {
                this.execute = execute;
            }

            // Implement the interface methods
            public void Execute(object parameter)
            {
                execute();
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

#pragma warning disable 67
            // This event is not used but it is required by the interface
            public event EventHandler CanExecuteChanged;
#pragma warning restore 67
        }

        #region INotifyPropertyChanged Required Functions and events

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propName = null)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        #endregion
    }
}
