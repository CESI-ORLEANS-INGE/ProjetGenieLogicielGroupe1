using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.Model
{
    public interface IStateFile
    {
        public void Save(List<IBackupJobState> jobsState);
        public List<IBackupJobState> Read();
    }
    public class StateFile 
    {

    }
}