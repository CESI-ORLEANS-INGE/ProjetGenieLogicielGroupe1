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
        public IStateFile File { get; set; }
        public List<IBackupJobState> JobState { get; set; }
        public IBackupJobState CreateJobState(IBackupJobState backupJobState);
        public void OnJobStateChanged();
    }
    public class BackupState 
    {
        public IStateFile File { get; set; }
        public List<IBackupJobState> JobState { get; set; }
        private static BackupState _backupState;
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
        private BackupState() { }
        public void OnJobStateChanged()
        {
            File.Save(JobState);
        }
        public IBackupJobState CreateJobState(IBackupJob backupJob)
        {
            IBackupJobState backupJobState = (IBackupJobState)new BackupJobState(backupJob);
            JobState.Add(backupJobState);
            backupJobState.JobStateChanged += (sender, args) => OnJobStateChanged();
            return backupJobState;
        }
        
    }
}