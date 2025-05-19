using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.Model {
    public class ProcessEventArgs(string processName) : EventArgs {
        public string ProcessName { get; } = processName;
    }
    public class ProcessesEventArgs(List<string> processes) : EventArgs {
        public List<string> Processes { get; } = processes;
    }

    public delegate void ProcessStartedEventHandler(object sender, ProcessEventArgs e);
    public delegate void ProcessEndedEventHandler(object sender, ProcessEventArgs e);
    public delegate void NoProcessRunningEventHandler(object sender, EventArgs e);
    public delegate void OneOrMoreProcessRunningEventHandler(object sender, ProcessesEventArgs e);

    public interface IProcessesDetector {
        public bool CheckProcesses();

        public event ProcessStartedEventHandler? ProcessStarded;
        public event ProcessEndedEventHandler? ProcessEnded;
        public event NoProcessRunningEventHandler? NoProcessRunning;
        public event OneOrMoreProcessRunningEventHandler? OneOrMoreProcessRunning;
    }

    public class ProcessesDetector : IProcessesDetector {
        private Dictionary<string, bool> Processes { get; set; } = [];
        private Task? Task { get; set; } = null!;

        public ProcessesDetector() {
            List<string> processes = Configuration.Instance?.Processes.ToList() ?? throw new Exception("Configuration is null");

            foreach (string process in processes) {
                this.Processes.Add(process, false);
            }

            this.Task = Task.Run(() => {
                while (true) {
                    if (!CheckProcesses()) {
                        NoProcessRunning?.Invoke(this, EventArgs.Empty);
                    } else {
                        OneOrMoreProcessRunning?.Invoke(this, new ProcessesEventArgs(this.Processes.Keys.ToList()));
                    }
                    Task.Delay(1000).Wait();
                }
            });

            if (Configuration.Instance is not null) {
                Configuration.Instance.ConfigurationChanged += OnConfigurationChanged;
            }
        }

        private void OnConfigurationChanged(object sender, ConfigurationChangedEventArgs e) {
            if (Configuration.Instance is null) return;
            if (e.PropertyName != nameof(Configuration.Instance.Processes)) return;

            this.Processes = Configuration.Instance.Processes.ToDictionary(process => process, process => this.Processes.TryGetValue(process, out bool value) && value);
        }

        public bool CheckProcesses() {
            List<Process> runningProcesses = [.. Process.GetProcesses()];

            foreach (string process in this.Processes.Keys) {
                bool isRunning = runningProcesses.Any(p => 
                    p.ProcessName.Equals(process, StringComparison.OrdinalIgnoreCase)
                );
                if (isRunning && !this.Processes[process]) {
                    this.Processes[process] = true;
                    ProcessStarded?.Invoke(this, new ProcessEventArgs(process));
                } else if (!isRunning && this.Processes[process]) {
                    this.Processes[process] = false;
                    ProcessEnded?.Invoke(this, new ProcessEventArgs(process));
                }
            }

            return this.Processes.Values.Any(p => p);
        }

        public event ProcessStartedEventHandler? ProcessStarded;
        public event ProcessEndedEventHandler? ProcessEnded;
        public event NoProcessRunningEventHandler? NoProcessRunning;
        public event OneOrMoreProcessRunningEventHandler? OneOrMoreProcessRunning;
    }
}
