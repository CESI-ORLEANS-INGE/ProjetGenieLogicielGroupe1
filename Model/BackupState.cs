using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.Model;

public interface IBackupState {
    /// <summary>
    /// Gets or sets the file responsible for saving the state.
    /// </summary>
    public IStateFile File { get; set; }
    /// <summary>
    /// Gets or sets the list of job states being tracked.
    /// </summary>
    public List<IBackupJobState> JobState { get; set; }
    /// <summary>
    /// Creates and adds a new backup job state from a given job,
    /// and subscribes to its JobStateChanged event for tracking changes.
    /// </summary>
    public IBackupJobState CreateJobState(IBackupJob backupJob);
    /// <summary>
    /// Called when any job state changes to persist the updated state.
    /// </summary>
    public void OnJobStateChanged();
}

/// <summary>
/// Singleton class that manages backup job states and persists them.
/// </summary>
public class BackupState : IBackupState {
    /// <summary>
    /// Provides access to the singleton instance of BackupState.
    /// </summary>
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
