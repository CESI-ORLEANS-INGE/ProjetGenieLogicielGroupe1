using EasySave.Logger;
using EasySave.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave;

public class LanguageChangedEventArgs(string language) : EventArgs
{
    public string? Language { get; set; } = language;
}
public class JobStateChangedEventArgs(IBackupJobState jobState) : EventArgs
{
    public IBackupJobState? JobState { get; set; } = jobState;
}

public delegate void LanguageChangedEventHandler(object sender, LanguageChangedEventArgs e);
public delegate void JobStateChangedEventHandler(object sender, JobStateChangedEventArgs e);
public delegate void ConfigurationChangedEventHandler(object sender, ConfigurationChangedEventArgs e);

public interface IViewModel : INotifyPropertyChanged
{

    /// <summary>
    /// List of all backup jobs.
    /// </summary>
    List<IBackupJob> BackupJobs { get; set; }

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
    /// Runs the command to change the log file path.
    /// </summary>
    void RunCommandLog(string logFilePath);

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
}

public class ViewModel : IViewModel
{
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

    public ViewModel()
    {
        ConfigurationManager configurationManager = new(typeof(ConfigurationJSONFile));
        this.Configuration = configurationManager.Load(ViewModel.CONFIGURATION_PATH);
        this.Configuration.ConfigurationChanged += this.OnConfigurationChanged;

        this.Language = Model.Language.Instance;
        this.Language.LanguageChanged += this.OnLanguageChanged;
        this.Language.Load();

        this.ProcessesDetector = new ProcessesDetector();
        this.ProcessesDetector.OneOrMoreProcessRunning += (sender, e) => {
            foreach (IBackupJob job in this.BackupJobs)
            {
                job.Stop();
                Logger?.Info(new Log
                {
                    JobName = job.Name,
                    Message = "One or more processes are running, stopping the backup job.",
                });
            }
        };

        this.Logger = new Logger.Logger(this.Configuration.LogFile);
    }

    public async void RunCommandRun(List<string> indexOrNameList)
    {
        const int MAX_CONCURRENT_JOBS = 10;

        // Utilisation de SemaphoreSlim au lieu de Mutex pour async/await
        using SemaphoreSlim mutex = new(1, 1);

        List<IBackupJobConfiguration> jobsToRun = [];

        foreach (string indexOrName in indexOrNameList)
        {
            // Check if the indexOrName is a number
            if (int.TryParse(indexOrName, out int id))
            {
                id = id - 1; // Adjust for 0-based index
                if (id < 0 || id >= this.Configuration.Jobs.Count)
                {
                    throw new Exception($"No backup job found with index: {indexOrName}");
                }
                else
                {
                    jobsToRun.Add(this.Configuration.Jobs[id]);
                }
            }
            else
            {
                IBackupJobConfiguration? job = this.Configuration.Jobs.FirstOrDefault(job => job.Name.Equals(indexOrName, StringComparison.OrdinalIgnoreCase));
                if (job is null)
                {
                    throw new Exception($"No backup job found with name: {indexOrName}");
                }
                else
                {
                    jobsToRun.Add(job);
                }
            }
        }

        // Check if there are any jobs to run
        if (jobsToRun.Count == 0)
        {
            throw new Exception("No backup jobs available.");
        }

        this.BackupJobs = BackupJobFactory.Create(jobsToRun);

        IStateFile file = new StateFile(this.Configuration.StateFile);
        using (this.BackupState = new BackupState(file))
        {
            this.BackupState.JobStateChanged += this.OnJobStateChanged;

            using SemaphoreSlim semaphore = new(MAX_CONCURRENT_JOBS);
            await Task.WhenAll([.. this.BackupJobs.Select(job => Task.Run(async () => {
                await semaphore.WaitAsync();
                try
                {
                    // Section critique protégée par le mutex
                    await mutex.WaitAsync();
                    try
                    {
                        job.Analyze();
                        this.BackupState.CreateJobState(job);
                    }
                    finally
                    {
                        mutex.Release();
                    }

                    await RunJobWithTransferControl(job);
                }
                finally
                {
                    semaphore.Release();
                }
            }))]);
        }
    }

    private async Task RunJobWithTransferControl(IBackupJob job)
    {
        // Estimation de la taille du job
        long estimatedSizeKB = GetEstimatedJobSizeKB(job);

        // Vérification si on peut démarrer ce transfert
        bool canStart = false;
        lock (_transferLock)
        {
            if (_totalTransferSizeKB + estimatedSizeKB <= MAX_TOTAL_TRANSFER_SIZE_KB)
            {
                _totalTransferSizeKB += estimatedSizeKB;
                canStart = true;
            }
        }

        if (!canStart)
        {
            Logger?.Warning(new Log
            {
                JobName = job.Name,
                Message = $"Transfert annulé - limite de {MAX_TOTAL_TRANSFER_SIZE_KB} KB dépassée. Taille actuelle: {_totalTransferSizeKB} KB, Taille demandée: {estimatedSizeKB} KB"
            });
            return;
        }

        try
        {
            Logger?.Info(new Log
            {
                JobName = job.Name,
                Message = $"Démarrage du transfert. Taille: {estimatedSizeKB} KB, Total en cours: {_totalTransferSizeKB} KB"
            });

            await job.Run();

            Logger?.Info(new Log
            {
                JobName = job.Name,
                Message = $"Transfert terminé avec succès. Taille: {estimatedSizeKB} KB"
            });
        }
        catch (Exception ex)
        {
            Logger?.Error(new Log
            {
                JobName = job.Name,
                Message = $"Erreur lors du transfert: {ex.Message}"
            });
            throw;
        }
        finally
        {
            // Libération de la taille utilisée
            lock (_transferLock)
            {
                _totalTransferSizeKB -= estimatedSizeKB;
                Logger?.Info(new Log
                {
                    JobName = job.Name,
                    Message = $"Libération de {estimatedSizeKB} KB. Total restant: {_totalTransferSizeKB} KB"
                });
            }
        }
    }

    private long GetEstimatedJobSizeKB(IBackupJob job)
    {
        try
        {
            if (job.Source != null && System.IO.Directory.Exists(job.Source.GetPath()))
            {
                var directoryInfo = new System.IO.DirectoryInfo(job.Source.GetPath());
                long totalBytes = GetDirectorySize(directoryInfo);
                return totalBytes / 1024;
            }
            else if (job.Source != null && System.IO.File.Exists(job.Source.GetPath()))
            {
                var fileInfo = new System.IO.FileInfo(job.Source.GetPath());
                return fileInfo.Length / 1024;
            }
        }
        catch (Exception ex)
        {
            Logger?.Warning(new Log
            {
                JobName = job.Name,
                Message = $"Impossible d'estimer la taille du job: {ex.Message}"
            });
        }

        return 102400; // 100 MB par défaut
    }

    private long GetDirectorySize(System.IO.DirectoryInfo directoryInfo)
    {
        long size = 0;
        try
        {
            foreach (System.IO.FileInfo file in directoryInfo.GetFiles())
            {
                size += file.Length;
            }
            foreach (System.IO.DirectoryInfo subDirectory in directoryInfo.GetDirectories())
            {
                size += GetDirectorySize(subDirectory);
            }
        }
        catch (UnauthorizedAccessException)
        {
            // Ignorer les dossiers inaccessibles
        }
        catch (System.IO.DirectoryNotFoundException)
        {
            // Ignorer les dossiers inexistants
        }
        return size;
    }

    // Méthode pour obtenir la taille totale actuelle (pour monitoring)
    public static long GetCurrentTotalTransferSize()
    {
        lock (_transferLock)
        {
            return _totalTransferSizeKB;
        }
    }

    // Méthode pour obtenir la limite maximale
    public static long GetMaxTotalTransferSize()
    {
        return MAX_TOTAL_TRANSFER_SIZE_KB;
    }

    public void RunCommandAdd(string name, string source, string destination, string type)
    {
        if (this.Configuration.Jobs.FirstOrDefault(job => job.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) is not null)
        {
            throw new Exception($"A backup job with the name '{name}' already exists.");
        }

        if (this.Configuration.Jobs.FirstOrDefault(job =>
            job.Source.Equals(source, StringComparison.OrdinalIgnoreCase) &&
            job.Destination.Equals(destination, StringComparison.OrdinalIgnoreCase) &&
            job.Type.Equals(type, StringComparison.OrdinalIgnoreCase)
         ) is not null)
        {
            throw new Exception($"A backup job with the same source, destination and type already exists.");
        }

        // Create a new backup job configuration
        IBackupJobConfiguration newJob = new BackupJobConfiguration
        {
            Name = name,
            Source = source,
            Destination = destination,
            Type = type
        };

        // Add the new job to the configuration
        this.Configuration.AddJob(newJob);
    }

    public void RunCommandRemove(string indexOrName)
    {
        if (this.Configuration.Jobs.Count == 0)
        {
            throw new Exception("No backup jobs available.");
        }

        IBackupJobConfiguration? jobToRemove = null;
        // Check if the indexOrName is a number
        if (int.TryParse(indexOrName, out int id))
        {
            id = id - 1; // Adjust for 0-based index
            if (id < 0 || id >= this.Configuration.Jobs.Count)
            {
                jobToRemove = this.Configuration.Jobs.FirstOrDefault(job => job.Name.Equals(indexOrName, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                // Remove the backup job by index
                jobToRemove = this.Configuration.Jobs[id];
            }
        }
        else
        {
            jobToRemove = this.Configuration.Jobs.FirstOrDefault(job => job.Name.Equals(indexOrName, StringComparison.OrdinalIgnoreCase));
        }

        if (jobToRemove is null)
        {
            throw new Exception($"No backup job found with name or index: {indexOrName}");
        }

        // Remove the backup job from the configuration
        this.Configuration.RemoveJob(jobToRemove);
    }

    public void RunCommandLanguage(string language)
    {
        this.Language.SetLanguage(language);
    }

    public void RunCommandLog(string logFilePath)
    {
        Configuration.LogFile = logFilePath;
        Logger.SetLogFile(logFilePath);
    }

    public void OnLanguageChanged(object sender, LanguageChangedEventArgs e)
    {
        this.LanguageChanged?.Invoke(this, e);
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Language)));
    }

    public void OnJobStateChanged(object sender, JobStateChangedEventArgs e)
    {
        this.JobStateChanged?.Invoke(this, e);

        switch (e.JobState?.State)
        {
            case State.IN_PROGRESS:
                IBackupTask task = e.JobState.BackupJob.Tasks[e.JobState.BackupJob.CurrentTaskIndex];
                this.Logger.Info(new Log
                {
                    JobName = e.JobState.BackupJob.Name,
                    Filesize = task.Source?.GetSize() ?? 0,
                    Source = task.Source?.GetPath() ?? string.Empty,
                    Destination = task.Destination?.GetPath() ?? string.Empty,
                    TaskType = task is BackupCopyTask ? "Copy" : "Remove",
                    TransfertDuration = task.GetDuration()
                });
                break;
            case State.CANCEL:
                Console.WriteLine("Job Cancelled");
                break;
        }
    }

    // Party configuration 🎉
    public string BLanguage
    {
        get => Configuration.Language;
        set
        {
            Language.SetLanguage(value);
            OnPropertyChanged(nameof(BLanguage));
            OnPropertyChanged(nameof(Language));
        }
    }

    public string StateFile
    {
        get => Configuration.StateFile;
        set
        {
            Configuration.StateFile = value;
            OnPropertyChanged(nameof(StateFile));
        }
    }

    public string LogFile
    {
        get => Configuration.LogFile;
        set
        {
            Configuration.LogFile = value;
            OnPropertyChanged(nameof(LogFile));
        }
    }

    public string CryptoFile
    {
        get => Configuration.CryptoFile;
        set
        {
            Configuration.CryptoFile = value;
            OnPropertyChanged(nameof(CryptoFile));
        }
    }

    public string ExtensionsToEncrypt
    {
        get => string.Join(";", Configuration.CryptoExtentions);
        set
        {
            Configuration.CryptoExtentions = [.. value.Split(';')];
            OnPropertyChanged(nameof(ExtensionsToEncrypt));
        }
    }

    public string EncryptionKey
    {
        get => Configuration.CryptoKey;
        set
        {
            Configuration.CryptoKey = value;
            OnPropertyChanged(nameof(EncryptionKey));
        }
    }

    public string Processes
    {
        get => string.Join(";", Configuration.Processes);
        set
        {
            Configuration.Processes = [.. value.Split(";")];
            OnPropertyChanged(nameof(Processes));
        }
    }

    public void OnConfigurationChanged(object sender, ConfigurationChangedEventArgs e)
    {
        this.ConfigurationChanged?.Invoke(this, e);

        if (e.PropertyName == nameof(IConfiguration.StateFile) && this.BackupState is not null)
        {
            this.BackupState.File = new StateFile(this.Configuration.StateFile);
        }
    }

    public void OnPropertyChanged(string propertyName)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public event LanguageChangedEventHandler? LanguageChanged;
    public event JobStateChangedEventHandler? JobStateChanged;
    public event ConfigurationChangedEventHandler? ConfigurationChanged;
}