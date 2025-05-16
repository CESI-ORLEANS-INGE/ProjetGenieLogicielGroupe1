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
    List<IBackupJob> BackupJobs { get; }

    /// <summary>
    /// Backup state
    /// </summary>
    IBackupState BackupState { get; set; }

    /// <summary>
    /// Language used in the application.
    /// </summary>
    ILanguage Language { get; }

    /// <summary>
    /// Configuration object containing the application settings.
    /// </summary>
    IConfiguration Configuration { get; }

    /// <summary>
    /// Runs the command to start the backup job.
    /// </summary>
    void RunCommandRun(List<string> indexOrNameList);

    /// <summary>
    /// Runs the command to add a new backup job.
    /// </summary>
    void RunCommandAdd(string name, string source, string destination, string type);

    /// <summary>
    /// Runs the command to remove a backup job.
    /// </summary>
    void RunCommandRemove(string indexOrName);

    /// <summary>
    /// Runs the command to change the application language.
    /// </summary>
    void RunCommandLanguage(string language);

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
}

public class ViewModel : IViewModel {
    public const string CONFIGURATION_PATH = "./configuration.json";

    public List<IBackupJob> BackupJobs { get; set; } = [];
    public IBackupState? BackupState { get; set; }
    public ILanguage Language { get; set; }
    public IConfiguration Configuration { get; set; }
    public ILogger Logger { get; set; }
    public IProcessesDetector ProcessesDetector { get; set; }

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
                job.Stop();
            }
        };

        this.Logger = new Logger.Logger(this.Configuration.LogFile);
    }

    public void RunCommandRun(List<string> indexOrNameList) {
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
        using (this.BackupState = new BackupState(file))
        this.BackupState.JobStateChanged += this.OnJobStateChanged;

        foreach (IBackupJob job in this.BackupJobs) {
            job.Analyze();
            this.BackupState.CreateJobState(job);
        }

        foreach (IBackupJob job in this.BackupJobs) {
            job.Run();
        }
    }
    public void RunCommandAdd(string name, string source, string destination, string type) {
        if (this.Configuration.Jobs.Count >= 5) {
            throw new Exception("Maximum number of backup jobs reached (5).");
        }
        
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
    public void RunCommandRemove(string indexOrName) {
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
    public void RunCommandLanguage(string language) {
        this.Language.SetLanguage(language);
    }


    public void OnLanguageChanged(object sender, LanguageChangedEventArgs e) {
        this.LanguageChanged?.Invoke(this, e);
    }
    public void OnJobStateChanged(object sender, JobStateChangedEventArgs e) {
        this.JobStateChanged?.Invoke(this, e);

        if (e.JobState?.State == State.IN_PROGRESS) {
            IBackupTask task = e.JobState.BackupJob.Tasks[e.JobState.BackupJob.CurrentTask];
            this.Logger.Info(new Log {
                JobName = e.JobState.BackupJob.Name,
                Filesize = task.Source?.GetSize() ?? 0,
                Source = task.Source?.GetPath() ?? string.Empty,
                Destination = task.Destination?.GetPath() ?? string.Empty,
                TaskType = task is BackupCopyTask ? "Copy" : "Remove",
                TransfertDuration = task.GetDuration()
            });
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

