using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace EasySave.Model {
    public enum State
    {
        ACTIVE, END, ERROR, IN_PROGRESS, BREAK, RESUMED
    }
    public interface IBackupJobState {
        public IBackupJob BackupJob { get; set; }
        public string SourceFilePath { get; set; }
        public string DestinationFilePath { get; set; }
        public State state { get; set; }
        public double TotalFilesToCopy { get; set; }
        public double TotalFilesSize { get; set; }
        public double FilesLeft { get; set; }
        public double FilesLeftSize { get; set; }
        public int Progression { get; set; }
        public void OnJobStarted(object sender, BackupJobEventArgs e);
        public void OnJobProgress(object sender, BackupJobEventArgs e);
        public void OnJobPaused(object sender, BackupJobEventArgs e);
        public void OnJobResumed(object sender, BackupJobEventArgs e);
        public void OnJobFinished(object sender, BackupJobEventArgs e);
        public void OnJobCancelled(object sender, BackupJobEventArgs e);
        public event EventHandler JobStateChanged;
    }
    public class IJobStateChangedEventArgs : EventArgs
    {
        public string? JobName { get; set; }
        public State NewState { get; set; }
    }

    public class BackupJobState {
        public IBackupJob BackupJob { get; set; }
        public string SourceFilePath { get; set; }
        public string DestinationFilePath { get; set; }
        public State State { get; set; }
        public double TotalFilesToCopy { get; set; }
        public double TotalFilesSize { get; set; }
        public double FilesLeft { get; set; }
        public double FilesLeftSize { get; set; }
        public int Progression { get; set; }

        public BackupJobState(IBackupJob backupJob)
        {
            SourceFilePath = backupJob.Source.GetPath();
            DestinationFilePath = backupJob.Destination.GetPath();
            TotalFilesToCopy = backupJob.Tasks.Count;
            TotalFilesSize = backupJob.Tasks.Where( t => t is BackupCopyTask).Sum(t => t.Source?.GetSize() ?? 0); 
            FilesLeft = TotalFilesToCopy;
            FilesLeftSize = TotalFilesSize;
            Progression = 0;
            

            // S'abonner aux événements de BackupJob
            backupJob.BackupJobStarted += OnJobStarted;
            backupJob.BackupJobProgress += OnJobProgress;
            backupJob.BackupJobPaused += OnJobPaused;
            backupJob.BackupJobResumed += OnJobResumed;
            backupJob.BackupJobFinished += OnJobFinished;
            backupJob.BackupJobCancelled += OnJobCancelled;
        }
        private void RaiseStateChanged()
        {
            JobStateChanged?.Invoke(this, new IJobStateChangedEventArgs
            {
                JobName = BackupJob.Name,
                NewState = this.State
            });
        }
        public void OnJobStarted(object sender, BackupJobEventArgs e) {

            State = State.ACTIVE;
            RaiseStateChanged();
        }
        public void OnJobProgress(object sender, BackupJobEventArgs e) {
            State = State.IN_PROGRESS;
            RaiseStateChanged();
        }
        public void OnJobPaused(object sender, BackupJobEventArgs e) {
            State = State.BREAK;
            RaiseStateChanged();
        }
        public void OnJobResumed(object sender, BackupJobEventArgs e) {
            State = State.RESUMED;
            RaiseStateChanged();
        }
        public void OnJobFinished(object sender, BackupJobEventArgs e) {
            State = State.END;
            RaiseStateChanged();
        }
        public void OnJobCancelled(object sender, BackupJobEventArgs e) {
            State = State.ERROR;
            RaiseStateChanged();
        }
        public event EventHandler<IJobStateChangedEventArgs>? JobStateChanged;

    }

}
