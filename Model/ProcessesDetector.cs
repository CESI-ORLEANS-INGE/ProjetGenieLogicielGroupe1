using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.Model
{
    public interface IProcessesDetector
    {
        public Dictionary<string,bool> Processes { get; }
        public Task task { get; }

        public bool CheckProcesses();

        public event EventHandler ProcessStarded;
        public event EventHandler ProcessEnded;
        public event EventHandler AllProcessEnded;
        public event EventHandler OneProcessRunning;
    }
    public class ProcessesDetector
    {
    }
}
