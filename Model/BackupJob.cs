using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.Model;

public class IBackupJobEventArgs(string jobName) : EventArgs {
    public string? JobName { get; } = jobName;
}
public class IBackupJobProgressEventArgs(string jobName, int progress) : IBackupJobEventArgs(jobName) {
    public int? Progress { get; } = progress;
}
public class IBackupJobErrorEventArgs(string jobName, string errorMesssage) : IBackupJobEventArgs(jobName) {
    public string? ErrorMessage { get; } = errorMesssage;
}
public class IBackupJobCancelledEventArgs(string jobName, string cancelMessage) : IBackupJobEventArgs(jobName) {
    public string? CancelMessage { get; } = cancelMessage;
}
public delegate void BackupJobEventHandler(object sender, IBackupJobEventArgs e);
public delegate void BackupJobProgressEventHandler(object sender, IBackupJobProgressEventArgs e);
public delegate void BackupJobErrorEventHandler(object sender, IBackupJobErrorEventArgs e);
public delegate void BackupJobCancelledEventHandler(object sender, IBackupJobCancelledEventArgs e);

public interface IBackupJob {
    /// <summary>
    /// Name of the backup job.
    /// </summary>
    public string Name { get; }
    /// <summary>
    /// Source directory handler for the backup job.
    /// This is the directory from which files will be backed up.
    /// </summary>
    public IDirectoryHandler Source { get; }
    /// <summary>
    /// Destination directory handler for the backup job.
    /// This is the directory where the backed up files will be stored.
    /// </summary>
    public IDirectoryHandler Destination { get; }
    /// <summary>
    /// List of backup tasks to be executed.
    /// </summary>
    public List<IBackupTask> Tasks { get; }
    /// <summary>
    /// Current task index being executed.
    /// This is used to track the progress of the backup job.
    /// </summary>
    public int CurrentTask { get; }

    /// <summary>
    /// Analyzes the source and destination directories to determine the files that need to be backed up.
    /// This method should be called before running the backup job.
    /// </summary>
    public abstract void Analyze();
    /// <summary>
    /// Runs the backup job.
    /// This method will execute the backup tasks in the order they are defined in the Tasks list.
    /// </summary>
    public void Run();

    ///// <summary>
    ///// Pauses the backup job.
    ///// </summary>
    //public void Pause();

    ///// <summary>
    ///// Resumes the backup job.
    ///// </summary>
    //public void Resume();

    ///// <summary>
    ///// Cancels the backup job.
    ///// </summary>
    //public void Cancel();

    public event BackupJobEventHandler? BackupJobStarted;
    public event BackupJobProgressEventHandler? BackupJobProgress;
    public event BackupJobEventHandler? BackupJobPaused;
    public event BackupJobEventHandler? BackupJobResumed;
    public event BackupJobEventHandler? BackupJobFinished;
    public event BackupJobErrorEventHandler? BackupJobError;
    public event BackupJobCancelledEventHandler? BackupJobCancelled;
}

public abstract class BackupJob(string name, IDirectoryHandler source, IDirectoryHandler destination) : IBackupJob {
    public string Name { get; } = name;
    public IDirectoryHandler Source { get; } = source;
    public IDirectoryHandler Destination { get; } = destination;

    public List<IBackupTask> Tasks { get; } = [];
    public int CurrentTask { get; set; } = 0;

    public event BackupJobEventHandler? BackupJobStarted;
    public event BackupJobProgressEventHandler? BackupJobProgress;
    public event BackupJobEventHandler? BackupJobPaused;
    public event BackupJobEventHandler? BackupJobResumed;
    public event BackupJobEventHandler? BackupJobFinished;
    public event BackupJobErrorEventHandler? BackupJobError;
    public event BackupJobCancelledEventHandler? BackupJobCancelled;

    public abstract void Analyze();

    public void Run() {
        this.BackupJobStarted?.Invoke(this, new IBackupJobEventArgs(this.Name));
        if (Tasks.Count == 0) {
            this.BackupJobFinished?.Invoke(this, new IBackupJobEventArgs(this.Name));
            return;
        }

        for (this.CurrentTask = 0; this.CurrentTask < Tasks.Count; this.CurrentTask++) {
            IBackupTask task = Tasks[this.CurrentTask];
            task.StartTime = DateTime.Now;
            try {
                task.Run();
                task.EndTime = DateTime.Now;
                this.BackupJobProgress?.Invoke(this, new IBackupJobProgressEventArgs(this.Name, (int)((this.CurrentTask + 1) * 100 / Tasks.Count)));
            } catch (Exception ex) {
                this.BackupJobError?.Invoke(this, new IBackupJobErrorEventArgs(this.Name, ex.Message));
                return;
            }
        }

        this.BackupJobFinished?.Invoke(this, new IBackupJobEventArgs(this.Name));
    }
}

