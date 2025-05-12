using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace EasySave.Model;

public interface IBackupState {
    public IStateFile File { get; set; }
    public List<IBackupJobState> JobState { get; set; }
    public IBackupJobState CreateJobState(IBackupJob backupJob);
    public void OnJobStateChanged();
}

public class BackupState : IBackupState {
    private static BackupState? Instance;

    public IStateFile File { get; set; }
    public List<IBackupJobState> JobState { get; set; } = [];

    public BackupState(IStateFile file) {
        if (BackupState.Instance != null) {
            throw new InvalidOperationException("BackupState is a singleton. Use Instance property to access it.");
        }

        this.File = file;

        BackupState.Instance = this;
    }

    public void OnJobStateChanged() {
        this.File?.Save(JobState);
    }
    public IBackupJobState CreateJobState(IBackupJob backupJob) {
        IBackupJobState backupJobState = new BackupJobState(backupJob);
        JobState.Add(backupJobState);
        backupJobState.JobStateChanged += (sender, args) => OnJobStateChanged();
        return backupJobState;
    }

}
