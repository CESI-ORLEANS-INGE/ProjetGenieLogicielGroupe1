using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyRemote.Model
{
    public enum State
    {
        
    }
    public interface IBackupJobState
{
    IBackupJob BackupJob { get; set; }
    public string SourceFilePath { get; set; }
    public string DestinationFilePath { get; set; }
    State State { get; set; }
    public double TotalFilesToCopy { get; set; }
    public double TotalFileSize { get; set; }
    public int Progression { get; set; }

}

    class BackupJobState : IBackupJobState
    {
        public IBackupJob BackupJob { get; set; }
        public string SourceFilePath { get; set; }
        public string DestinationFilePath { get; set; }
        public State State { get; set; }
        public double TotalFilesToCopy { get; set; }
        public double TotalFileSize { get; set; }
        public int Progression { get; set; }
    }
}
