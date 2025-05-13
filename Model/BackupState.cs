using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace EasySave.Model
{
    public interface IBackupState 
    {
        /// <summary>
        /// Gets or sets the file responsible for saving the state.
        /// </summary>
        public IStateFile File { get; set; }
        /// <summary>
        /// Gets or sets the list of job states being tracked.
        /// </summary>
        public List<IBackupJobState> JobState { get; set; }
        /// <summary>
        /// Adds a new backup job state and subscribes to its change events.
        /// </summary>
        public IBackupJobState CreateJobState(IBackupJobState backupJobState);
        /// <summary>
        /// Called when any job state changes to persist the updated state.
        /// </summary>
        public void OnJobStateChanged();
    }
    /// <summary>
    /// Singleton class that manages backup job states and persists them.
    /// </summary>
    public class BackupState 
    {
        public IStateFile File { get; set; }
        public List<IBackupJobState> JobState { get; set; }
        /// <summary>
        /// Singleton instance of the BackupState.
        /// </summary>
        private static BackupState _backupState;
        /// <summary>
        /// Provides access to the singleton instance of BackupState.
        /// Initializes the instance if it hasn't been created.
        /// </summary>
        public static BackupState backupState
        {
            get
            {
                if (_backupState == null)
                {
                    _backupState = new BackupState();
                }
                return _backupState;
            }
        }
        /// <summary>
        /// Private constructor to prevent external instantiation.
        /// </summary>
        private BackupState() { }
        /// <summary>
        /// Persists the current state of all backup jobs using the associated state file.
        /// </summary>
        public void OnJobStateChanged()
        {
            File.Save(JobState);
        }
        /// <summary>
        /// Creates and adds a new backup job state from a given job,
        /// and subscribes to its JobStateChanged event for tracking changes.
        /// </summary>
        public IBackupJobState CreateJobState(IBackupJob backupJob)
        {
            IBackupJobState backupJobState = (IBackupJobState)new BackupJobState(backupJob);
            JobState.Add(backupJobState);
            backupJobState.JobStateChanged += (sender, args) => OnJobStateChanged();
            return backupJobState;
        }
        
    }
}