using EasySave.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave;

public interface IViewModel : INotifyPropertyChanged {
    List<IBackupJob> BackupJobs { get; }
    List<IBackupJobState> BackupJobStates { get; }
    ILanguage Language { get; }
    Configuration Configuration { get; }
    void RunMain();
    void RunCommandRun();
    void RunCommandList();
    void RunCommandAdd();
    void RunCommandRemove();
    void RunCommandLanguage();
    List<string> ParseCommand(string command);
    void OnLanguageChanged(string language);
    void OnJobStateChanged(IBackupJobState jobState);
    void OnConfigurationChanged();
    void OnPropertyChanged(string propertyName);
}
internal class ViewModel {
}

 