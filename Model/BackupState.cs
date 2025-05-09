using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace EasySave.Model
{
    public interface IBackupState 
    {
        IStateFile File { get; set}
        List<IBackupJobState> JobState { get; set; }
        IBackupJobState CreateJobState(IBackupJobState backupJobState);
        void OnJobStateChanged();
    }
    public class BackupState 
    {

    }
}