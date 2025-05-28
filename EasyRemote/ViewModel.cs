using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyRemote
{
    public interface IViewModel
    {
        IClientControler ClientControler { get; }
        List<IBackupJob> Jobs { get; }
        List<IBackupJobState> JobsState { get; }
        public void ParseCommand();
        public void OnPropertyChanged();
    }
    class ViewModel
    {
    }
}
