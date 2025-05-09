using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.Model
{
    public interface IStateFile
    {
        void Save(List<IBackupJobState> jobsState);
        List<IBackupJobState> Read();
    }
    public class StateFile 
    {

    }
}