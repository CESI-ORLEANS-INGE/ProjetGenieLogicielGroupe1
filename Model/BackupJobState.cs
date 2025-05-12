using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace EasySave.Model {
    /// <summary>
    /// Represents the various states a backup job can be in.
    /// </summary>
    public enum State
    {
        /// <summary>
        /// The job is created and ready to run.
        /// </summary>
        ACTIVE,
        /// <summary>
        /// The job has completed successfully.
        /// </summary>
        END,
        /// <summary>
        /// An error occurred during the job execution.
        /// </summary>
        ERROR,
        /// <summary>
        /// The job is currently running.
        /// </summary>
        IN_PROGRESS,
        /// <summary>
        /// The job is paused.
        /// </summary>
        BREAK,
        /// <summary>
        /// The job has resumed after a pause.
        /// </summary>
        RESUMED
    }
    /// <summary>
    /// Represents the state of a backup job at a given time.
    /// Tracks progress, size, and lifecycle events.
    /// </summary>
    public interface IBackupJobState {
        /// <summary>
        /// The backup job associated with this state instance.
        /// </summary>
        public IBackupJob BackupJob { get; set; }
        /// <summary>
        /// The path of the source directory being backed up.
        /// </summary>
        public string SourceFilePath { get; set; }
        /// <summary>
        /// The path of the destination directory where files are backed up.
        /// </summary>
        public string DestinationFilePath { get; set; }
        /// <summary>
        /// Current state of the backup job.
        /// </summary>
        public State state { get; set; }
        /// <summary>
        /// Total number of files to copy in this job.
        /// </summary>
        public double TotalFilesToCopy { get; set; }
        /// <summary>
        /// Total size of files to copy (in bytes).
        /// </summary>
        public double TotalFilesSize { get; set; }
        /// <summary>
        /// Number of files remaining to copy.
        /// </summary>
        public double FilesLeft { get; set; }
        /// <summary>
        /// Size of files left to copy (in bytes).
        /// </summary>
        public double FilesLeftSize { get; set; }
        /// <summary>
        /// Current progression of the job, from 0 to 100.
        /// </summary>
        public int Progression { get; set; }

        /// <summary>
        /// Called when the job starts.
        /// Used to initialize state.
        /// </summary>
        public void OnJobStarted(object sender, BackupJobEventArgs e);
        /// <summary>
        /// Called when progress is made in the backup job.
        /// Used to update progress and remaining files.
        /// </summary>
        public void OnJobProgress(object sender, BackupJobEventArgs e);
        /// <summary>
        /// Called when the job is paused.
        /// </summary>
        public void OnJobPaused(object sender, BackupJobEventArgs e);
        /// <summary>
        /// Called when the job resumes from a pause.
        /// </summary>
        public void OnJobResumed(object sender, BackupJobEventArgs e);
        /// <summary>
        /// Called when the job finishes successfully.
        /// </summary>
        public void OnJobFinished(object sender, BackupJobEventArgs e);
        /// <summary>
        /// Called when the job is cancelled by the user or system.
        /// </summary>
        public void OnJobCancelled(object sender, BackupJobEventArgs e);
        /// <summary>
        /// Event triggered whenever the state of the job changes.
        /// This can be used for saving state or updating the UI.
        /// </summary>
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
            /// <summary>
            /// Sets the source file path using the backup job's source directory.
            /// </summary>
            SourceFilePath = backupJob.Source.GetPath();
            /// <summary>
            /// Sets the destination file path using the backup job's destination directory.
            /// </summary>
            DestinationFilePath = backupJob.Destination.GetPath();
            /// <summary>
            /// Calculates the total number of files to copy based on the number of tasks.
            /// </summary>
            TotalFilesToCopy = backupJob.Tasks.Count;
            /// <summary>
            /// Calculates the total size of files to copy, summing only the sizes of BackupCopyTask instances.
            /// </summary>
            TotalFilesSize = backupJob.Tasks.Where( t => t is BackupCopyTask).Sum(t => t.Source?.GetSize() ?? 0);
            /// <summary>
            /// Initializes the remaining files count to the total number of files.
            /// </summary>
            FilesLeft = TotalFilesToCopy;
            /// <summary>
            /// Initializes the remaining size to the total size of the files to copy.
            /// </summary>
            FilesLeftSize = TotalFilesSize;
            /// <summary>
            /// Initializes the progression percentage to 0 at the start of the job.
            /// </summary>
            Progression = 0;


            /// <summary>
            /// Subscribes to backup job events to track and update state changes.
            /// </summary>
            backupJob.BackupJobStarted += OnJobStarted;
            backupJob.BackupJobProgress += OnJobProgress;
            backupJob.BackupJobPaused += OnJobPaused;
            backupJob.BackupJobResumed += OnJobResumed;
            backupJob.BackupJobFinished += OnJobFinished;
            backupJob.BackupJobCancelled += OnJobCancelled;
        }

        // <summary>
        /// Raises the <c>JobStateChanged</c> event to notify subscribers of a state change in the backup job.
        /// This is typically called after a change in job progress, pause, resume, finish, or cancellation.
        /// </summary>
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
