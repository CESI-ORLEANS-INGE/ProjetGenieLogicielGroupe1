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

    }
}