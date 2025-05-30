using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyRemote.Model;

namespace EasyRemote
{
    public interface IViewModel
    {
        IClientControler ClientControler { get; }
        List<IBackupJob> Jobs { get; }
        List<IBackupJobState> JobsState { get; }
        public void OnPropertyChanged(string propertyName);
    }
    class ViewModel
    {
        IClientControler ClientControler { get; }
        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
