using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyRemote
{ 
    public interface IBackupJobState
{
    IBackupJob BackupJob { get; set; }
    public string SourceFilePath { get; set; }
    public string DestinationFilePath { get; set; }
    State State { get; set; }
    public double TotalFilesToCopy { get; set; }
    public double TotalFileSize { get; set; }
    int Progression { get; set; }

}

    class BackupJobState
    {
        IBackupJob BackupJob { get; set; }
        public string SourceFilePath { get; set; }
        public string DestinationFilePath { get; set; }
        State State { get; set; }
        public double TotalFilesToCopy { get; set; }
        public double TotalFileSize { get; set; }
        int Progression { get; set; }
    }
}
