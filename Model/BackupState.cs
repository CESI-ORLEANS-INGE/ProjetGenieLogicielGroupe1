using System;
using System.Collections.Generic;
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
        public void OnJobStateChanged()
        {
            File.Save(JobState);
        }
        public IBackupJobState CreateJobState(IBackupJobState backupJobState)
        {
            JobState.Add(backupJobState);
            backupJobState.JobStateChanged += (sender, args) => OnJobStateChanged();
            return backupJobState;
        }
        
    }
}