using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace EasySave.Model {
    public enum State
    {
        ACTIVE, END, ERROR
    }
    public interface IBackupJobState {
        IBackupJob BackupJob { get; set; }
        public string SourceFilePath { get; set; }
        public string DestinationFilePath { get; set; }
        State State { get; set; }
        public double TotalFilesToCopy { get; set; }
        public double TotalFilesSize { get; set; }
        public double FilesLeft { get; set; }
        public double FilesLeftSize { get; set; }
        public int Progression { get; set; }
        public void OnJobStarted();
        public void OnJobProgress();
        public void OnJobPaused();
        public void OnJobResumed();
        public void OnJobFinished();
        public void OnJobCancelled();
        public event EventHandler JobStateChanged;
    }
    internal class BackupJobState {
        
    }
}
