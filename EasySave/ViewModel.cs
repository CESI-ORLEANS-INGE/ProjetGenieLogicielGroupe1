using EasySave.Helpers;
using EasySave.Logger;
using EasySave.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave;

public class LanguageChangedEventArgs(string language) : EventArgs {
    public string? Language { get; set; } = language;
}
public class JobStateChangedEventArgs(IBackupJobState jobState) : EventArgs {
    public IBackupJobState? JobState { get; set; } = jobState;
}

public delegate void LanguageChangedEventHandler(object sender, LanguageChangedEventArgs e);
public delegate void JobStateChangedEventHandler(object sender, JobStateChangedEventArgs e);
public delegate void ConfigurationChangedEventHandler(object sender, ConfigurationChangedEventArgs e);

public interface IViewModel : INotifyPropertyChanged {

    /// <summary>
    /// List of all backup jobs.
    /// </summary>
    List<IBackupJob> BackupJobs { get; set; }

    /// <summary>
    /// Backup state
    /// </summary>
    IBackupState? BackupState { get; set; }

    /// <summary>
    /// Language used in the application.
    /// </summary>
    ILanguage Language { get; }

    /// <summary>
    /// Configuration object containing the application settings.
    /// </summary>
    IConfiguration Configuration { get; }

    ILogger Logger { get; }

    /// <summary>
    /// Called when the language is changed.
    /// </summary>
    void OnLanguageChanged(object sender, LanguageChangedEventArgs e);

    /// <summary>
    /// Called when the job state is changed.
    /// </summary>
    void OnJobStateChanged(object sender, JobStateChangedEventArgs e);

    /// <summary>
    /// Called when the configuration is changed.
    /// </summary>
    void OnConfigurationChanged(object sender, ConfigurationChangedEventArgs e);

    /// <summary>
    /// Called when a property is changed.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed.</param>
    void OnPropertyChanged(string propertyName);

    event LanguageChangedEventHandler? LanguageChanged;
    event JobStateChangedEventHandler? JobStateChanged;
    event ConfigurationChangedEventHandler? ConfigurationChanged;

    // Party configuration 🎉

    string BLanguage { get; set; }
    string StateFile { get; set; }
    string LogFile { get; set; }
    string CryptoFile { get; set; }
    string ExtensionsToEncrypt { get; set; }
    string EncryptionKey { get; set; }
    string Processes { get; set; }
    Commands Commands { get; }
}

public class ViewModel : IViewModel {
    private const long MAX_TOTAL_TRANSFER_SIZE_KB = 1024000;
    private static long _totalTransferSizeKB = 0;
    private static readonly object _transferLock = new object();
    public const string CONFIGURATION_PATH = "./configuration.json";

    public List<IBackupJob> BackupJobs { get; set; } = [];
    public IBackupState? BackupState { get; set; }
    public ILanguage Language { get; set; }
    public IConfiguration Configuration { get; set; }
    public ILogger Logger { get; set; }
    public IProcessesDetector ProcessesDetector { get; set; }

    private ICrypto Crypto { get; set; }

    private SocketServer SocketServer { get; set; }

    public Commands Commands { get; } = new();

    public ViewModel() {
        ConfigurationManager configurationManager = new(typeof(ConfigurationJSONFile));
        this.Configuration = configurationManager.Load(ViewModel.CONFIGURATION_PATH);
        this.Configuration.ConfigurationChanged += this.OnConfigurationChanged;

        this.Language = Model.Language.Instance;
        this.Language.LanguageChanged += this.OnLanguageChanged;
        this.Language.Load();

        this.ProcessesDetector = new ProcessesDetector();
        this.ProcessesDetector.OneOrMoreProcessRunning += (sender, e) => {
            foreach (IBackupJob job in this.BackupJobs) {
                job.Pause();
                Logger?.Info(new Log {
                    JobName = job.Name,
                    Message = "One or more processes are running, stopping the backup job.",
                });
            }
        };

        this.ProcessesDetector.NoProcessRunning += (sender, e) => {
            foreach (IBackupJob job in this.BackupJobs) {
                job.Resume();
                Logger?.Info(new Log {
                    JobName = job.Name,
                    Message = "No processes are running, resuming the backup job.",
                });
            }
        };

        this.Logger = new Logger.Logger(this.Configuration.LogFile);

        this.Crypto = new Crypto(this.Configuration.CryptoFile, this.Configuration.CryptoKey);

        this.RegisterCommands();

        this.SocketServer = new SocketServer(this);
    }

    public void RegisterCommands() {
        this.Commands.RegisterCommand("run", (command) => this.RunCommandRun(command.Arguments), this.ParseJobList);
        this.Commands.RegisterCommand("add", (command) => {
            if (command.Arguments.Count < 4) {
                throw new Exception(this.Language.Translations["INVALID_INPUT"]);
            }
            this.RunCommandAdd(command.Arguments[0], command.Arguments[1], command.Arguments[2], command.Arguments[3]);
        });
        this.Commands.RegisterCommand("remove", (command) => {
            if (command.Arguments.Count < 1) {
                throw new Exception(this.Language.Translations["INVALID_INPUT"]);
            }
            this.RunCommandRemove(command.Arguments[0]);
        });
        this.Commands.RegisterCommand("language", (command) => {
            if (command.Arguments.Count < 1) {
                throw new Exception(this.Language.Translations["INVALID_INPUT"]);
            }
            this.RunCommandLanguage(command.Arguments[0]);
        });
        this.Commands.RegisterCommand("log", (command) => {
            if (command.Arguments.Count < 1) {
                throw new Exception(this.Language.Translations["INVALID_INPUT"]);
            }
            this.RunCommandLog(command.Arguments[0]);
        });
        this.Commands.RegisterCommand("pause", (command) => {
            if (command.Arguments.Count < 1) {
                throw new Exception(this.Language.Translations["INVALID_INPUT"]);
            }
            this.RunCommandPause(command.Arguments[0]);
        });
        this.Commands.RegisterCommand("resume", (command) => {
            if (command.Arguments.Count < 1) {
                throw new Exception(this.Language.Translations["INVALID_INPUT"]);
            }
            this.RunCommandResume(command.Arguments[0]);
        });
    }

    private List<string> ParseJobList(string jobList) {
        List<string> indexOrNameList = [];

        foreach (string indexOrName in jobList.Split(',')) {
            if (indexOrName.Contains('-')) {
                string[] indexes = indexOrName.Split('-');
                if (int.TryParse(indexes[0], out int first) && int.TryParse(indexes[1], out int last)) {
                    (first, last) = (Math.Min(first, last), Math.Max(first, last));
                    for (int i = first; i <= last; i++) {
                        if (!indexOrNameList.Contains(i.ToString())) {
                            indexOrNameList.Add(i.ToString());
                        }
                    }
                } else {
                    throw new Exception(this.Language.Translations["INVALID_INPUT"] + ": " + (string)indexOrName);
                }
            } else {
                if (!indexOrNameList.Contains(indexOrName)) {
                    indexOrNameList.Add(indexOrName);
                }
            }
        }

        return indexOrNameList;
    }

    private async void RunCommandRun(List<string> indexOrNameList) {
        // Utilisation de SemaphoreSlim au lieu de Mutex pour async/await
        using SemaphoreSlim mutex = new(1, 1);

        List<IBackupJobConfiguration> jobsToRun = [];

        foreach (string indexOrName in indexOrNameList) {
            // Check if the indexOrName is a number
            if (int.TryParse(indexOrName, out int id)) {
                id = id - 1; // Adjust for 0-based index
                if (id < 0 || id >= this.Configuration.Jobs.Count) {
                    throw new Exception($"No backup job found with index: {indexOrName}");
                } else {
                    jobsToRun.Add(this.Configuration.Jobs[id]);
                }
            } else {
                IBackupJobConfiguration? job = this.Configuration.Jobs.FirstOrDefault(job => job.Name.Equals(indexOrName, StringComparison.OrdinalIgnoreCase));
                if (job is null) {
                    throw new Exception($"No backup job found with name: {indexOrName}");
                } else {
                    jobsToRun.Add(job);
                }
            }
        }

        // Check if there are any jobs to run
        if (jobsToRun.Count == 0) {
            throw new Exception("No backup jobs available.");
        }

        this.BackupJobs = BackupJobFactory.Create(jobsToRun);

        IStateFile file = new StateFile(this.Configuration.StateFile);
        using (this.BackupState = new BackupState(file)) {

            this.BackupState.JobStateChanged += this.OnJobStateChanged;

            SemaphoreSlim semaphore = new(this.Configuration.MaxConcurrentJobs);
            await Task.WhenAll([.. this.BackupJobs.Select(job => Task.Run(async () => {
                await semaphore.WaitAsync();
                try {
                    job.Analyze();
                    this.BackupState.CreateJobState(job);
                    Task task = job.Run();
                    if (this.ProcessesDetector.HasOneOrMoreProcessRunning()) job.Pause();
                    await task;
                } finally {
                    semaphore.Release();
                }
            }))]);
        }
    }
    private void RunCommandAdd(string name, string source, string destination, string type) {
        if (this.Configuration.Jobs.FirstOrDefault(job => job.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) is not null) {
            throw new Exception($"A backup job with the name '{name}' already exists.");
        }

        if (this.Configuration.Jobs.FirstOrDefault(job =>
            job.Source.Equals(source, StringComparison.OrdinalIgnoreCase) &&
            job.Destination.Equals(destination, StringComparison.OrdinalIgnoreCase) &&
            job.Type.Equals(type, StringComparison.OrdinalIgnoreCase)
         ) is not null) {
            throw new Exception($"A backup job with the same source, destination and type already exists.");
        }

        // Create a new backup job configuration
        IBackupJobConfiguration newJob = new BackupJobConfiguration {
            Name = name,
            Source = source,
            Destination = destination,
            Type = type
        };

        // Add the new job to the configuration
        this.Configuration.AddJob(newJob);
    }
    private void RunCommandRemove(string indexOrName) {
        if (this.Configuration.Jobs.Count == 0) {
            throw new Exception("No backup jobs available.");
        }

        IBackupJobConfiguration? jobToRemove = null;
        // Check if the indexOrName is a number
        if (int.TryParse(indexOrName, out int id)) {
            id = id - 1; // Adjust for 0-based index
            if (id < 0 || id >= this.Configuration.Jobs.Count) {
                jobToRemove = this.Configuration.Jobs.FirstOrDefault(job => job.Name.Equals(indexOrName, StringComparison.OrdinalIgnoreCase));
            } else {
                // Remove the backup job by index
                jobToRemove = this.Configuration.Jobs[id];
            }
        } else {
            jobToRemove = this.Configuration.Jobs.FirstOrDefault(job => job.Name.Equals(indexOrName, StringComparison.OrdinalIgnoreCase));
        }

        if (jobToRemove is null) {
            throw new Exception($"No backup job found with name or index: {indexOrName}");
        }

        // Remove the backup job from the configuration
        this.Configuration.RemoveJob(jobToRemove);
    }
    private void RunCommandLanguage(string language) {
        this.Language.SetLanguage(language);
    }
    private void RunCommandLog(string logFilePath) {
        Configuration.LogFile = logFilePath;
        Logger.SetLogFile(logFilePath);
    }
    private void RunCommandPause(string nameOrId) {
        if (this.BackupState is null) {
            return;
        }
        for (int i = 0; i < this.BackupState.JobState.Count; i++) {
            IBackupJobState jobState = this.BackupState.JobState[i];
            if (jobState.BackupJob.Name.Equals(nameOrId, StringComparison.OrdinalIgnoreCase) || int.TryParse(nameOrId, out int id) && id - 1 == i
            ) {
                jobState.BackupJob.Pause();
            }
        }
    }
    private void RunCommandResume(string nameOrId) {
        if (this.BackupState is null) {
            return;
        }
        for (int i = 0; i < this.BackupState.JobState.Count; i++) {
            IBackupJobState jobState = this.BackupState.JobState[i];
            if (jobState.BackupJob.Name.Equals(nameOrId, StringComparison.OrdinalIgnoreCase) || int.TryParse(nameOrId, out int id) && id - 1 == i) {
                jobState.BackupJob.Resume();
            }
        }
    }

    public void OnLanguageChanged(object sender, LanguageChangedEventArgs e) {
        this.LanguageChanged?.Invoke(this, e);
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Language)));
    }

    public void OnJobStateChanged(object sender, JobStateChangedEventArgs e) {
        this.JobStateChanged?.Invoke(this, e);

        switch (e.JobState?.State) {
            case State.IN_PROGRESS:
                IBackupTask task = e.JobState.BackupJob.Tasks[e.JobState.BackupJob.CurrentTaskIndex];
                this.Logger.Info(new Log {
                    JobName = e.JobState.BackupJob.Name,
                    Filesize = task.Source?.GetSize() ?? 0,
                    Source = task.Source?.GetPath() ?? string.Empty,
                    Destination = task.Destination?.GetPath() ?? string.Empty,
                    TaskType = task is BackupCopyTask ? "Copy" : "Remove",
                    TransfertDuration = task.GetDuration()
                });
                break;
            case State.CANCEL:
                this.Logger.Info(new Log {
                    JobName = e.JobState.BackupJob.Name,
                    Message = "Backup job was cancelled."
                });
                break;
            case State.ERROR:
                this.Logger.Error(new Log {
                    JobName = e.JobState.BackupJob.Name,
                    Message = "An error occurred during the backup job."
                });
                break;

        }
    }

    // Party configuration 🎉
    public string BLanguage {
        get => Configuration.Language;
        set {
            Language.SetLanguage(value);
            OnPropertyChanged(nameof(BLanguage));
            OnPropertyChanged(nameof(Language));
        }
    }

    public string StateFile {
        get => Configuration.StateFile;
        set {
            Configuration.StateFile = value;
            OnPropertyChanged(nameof(StateFile));
        }
    }

    public string LogFile {
        get => Configuration.LogFile;
        set {
            Configuration.LogFile = value;
            OnPropertyChanged(nameof(LogFile));
        }
    }

    public string CryptoFile {
        get => Configuration.CryptoFile;
        set {
            Configuration.CryptoFile = value;
            OnPropertyChanged(nameof(CryptoFile));
        }
    }

    public string ExtensionsToEncrypt {
        get => string.Join(";", Configuration.CryptoExtentions);
        set {
            Configuration.CryptoExtentions = [.. value.Split(';')];
            OnPropertyChanged(nameof(ExtensionsToEncrypt));
        }
    }

    public string EncryptionKey {
        get => Configuration.CryptoKey;
        set {
            Configuration.CryptoKey = value;
            OnPropertyChanged(nameof(EncryptionKey));
        }
    }

    public string Processes {
        get => string.Join(";", Configuration.Processes);
        set {
            Configuration.Processes = [.. value.Split(";")];
            OnPropertyChanged(nameof(Processes));
        }
    }

    public void OnConfigurationChanged(object sender, ConfigurationChangedEventArgs e) {
        this.ConfigurationChanged?.Invoke(this, e);

        if (e.PropertyName == nameof(IConfiguration.StateFile) && this.BackupState is not null) {
            this.BackupState.File = new StateFile(this.Configuration.StateFile);
        }
    }

    public void OnPropertyChanged(string propertyName) {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public event LanguageChangedEventHandler? LanguageChanged;
    public event JobStateChangedEventHandler? JobStateChanged;
    public event ConfigurationChangedEventHandler? ConfigurationChanged;
}