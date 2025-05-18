using EasySave.Model;
using System.Data;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows;

namespace EasySave;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application {
    private readonly IViewModel _ViewModel;

    public App() {
        this._ViewModel = new ViewModel();
    }

    public List<string> ParseJobList(string jobList) {
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
                    throw new Exception(Language.Instance.Translations["INVALID_INPUT"] + ": " + (string)indexOrName);
                }
            } else {
                if (!indexOrNameList.Contains(indexOrName)) {
                    indexOrNameList.Add(indexOrName);
                }
            }
        }

        return indexOrNameList;
    }

    public void RunCommandRun(List<string> indexOrNameList) {
        this._ViewModel.RunCommandRun(indexOrNameList);
    }

    public void RunCommandList() {
        if (this._ViewModel is null) {
            throw new Exception("ViewModel is not initialized.");
        }
        for (int i = 0; i < this._ViewModel.Configuration.Jobs.Count; i++) {
            IBackupJobConfiguration job = this._ViewModel.Configuration.Jobs[i];
            string prefix = new(' ', ((string.Empty + (i + 1))).Length);
            Console.WriteLine(i + 1 + ". " + Language.Instance.Translations["JOB_NAME"] + ": " + job.Name);
            Console.WriteLine(prefix + ". " + Language.Instance.Translations["JOB_SOURCE"] + ": " + job.Source);
            Console.WriteLine(prefix + ". " + Language.Instance.Translations["JOB_DESTINATION"] + ": " + job.Destination);
            Console.WriteLine(prefix + ". " + Language.Instance.Translations["JOB_TYPE"] + ": " + job.Type);
            Console.WriteLine(string.Empty);
        }
    }

    public void RunCommandAdd(string name, string source, string destination, string type) {
        this._ViewModel.RunCommandAdd(name, source, destination, type);
    }

    public void RunCommandRemove(string indexOrName) {
        this._ViewModel.RunCommandRemove(indexOrName);
    }

    public void RunCommandLanguage(string language) {
        this._ViewModel.RunCommandLanguage(language);
    }

    public void RunCommandConfiguration() {
        var jsonObject = Configuration.Instance?.ToJSON() ?? [];

        string jsonString = jsonObject.ToJsonString(new JsonSerializerOptions {
            WriteIndented = true
        });

        Console.WriteLine(jsonString);
    }

    public void RunCommandLog(string filePath) {
        this._ViewModel.RunCommandLog(filePath);
    }

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern bool AllocConsole();
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern bool AttachConsole(int dwProcessId);
    private const int ATTACH_PARENT_PROCESS = -1;

    public void ApplicationStartup(object sender, StartupEventArgs e) {
        string[] args = e.Args;

        if (args.Length > 0) {
            App.AttachConsole(App.ATTACH_PARENT_PROCESS);
            this._ViewModel.LanguageChanged += this.OnLanguageChanged;
            this._ViewModel.JobStateChanged += this.OnJobStateChanged;
            this._ViewModel.ConfigurationChanged += this.OnConfigurationChanged;

            switch (args[0].ToLower()) {
                case "list":
                    this.RunCommandList();
                    break;
                case "run":
                    if (args.Length < 2) {
                        Console.WriteLine(Language.Instance.Translations["INVALID_INPUT"]);
                        break;
                    }
                    this.RunCommandRun(this.ParseJobList(args[1]));
                    break;
                case "add":
                    if (args.Length < 5) {
                        Console.WriteLine(Language.Instance.Translations["INVALID_INPUT"]);
                        break;
                    }
                    this.RunCommandAdd(args[1], args[2], args[3], args[4]);
                    break;
                case "remove":
                    if (args.Length < 2) {
                        Console.WriteLine(Language.Instance.Translations["INVALID_INPUT"]);
                        break;
                    }
                    this.RunCommandRemove(args[1]);
                    break;
                case "language":
                    if (args.Length < 2) {
                        Console.WriteLine(Language.Instance.Translations["INVALID_INPUT"]);
                        break;
                    }
                    this.RunCommandLanguage(args[1]);
                    break;
                case "configuration":
                case "config":
                    this.RunCommandConfiguration();
                    break;
                case "log":
                    if (args.Length < 2) {
                        Console.WriteLine(Language.Instance.Translations["INVALID_INPUT"]);
                        break;
                    }
                    this.RunCommandLog(args[1]);
                    break;
                default:
                    Console.WriteLine(Language.Instance.Translations["UNKNOWN_COMMAND"]);
                    break;
            }

            this.Shutdown();
        } else {
            // DEV
            App.AllocConsole();
            MainWindow mainWindow = new(this._ViewModel);
            mainWindow.Show();
        }
    }

    public void OnLanguageChanged(object sender, LanguageChangedEventArgs e) {
        Console.WriteLine(Language.Instance.Translations["LANGUAGE_CHANGED"] + ": " + e.Language);
    }

    public void OnJobStateChanged(object sender, JobStateChangedEventArgs e) {
        switch (e.JobState?.State) {
            case State.ACTIVE:
                Console.WriteLine(Language.Instance.Translations["JOB_STATE_STARTED"] + " : " + e.JobState.BackupJob.Name);
                break;
            case State.IN_PROGRESS:
                Console.WriteLine(
                    Language.Instance.Translations["JOB_STATE_IN_PROGRESS"] +
                    " : " + e.JobState.BackupJob.Name + " => " + e.JobState.Progression + "% "
                );
                break;
            case State.END:
                Console.WriteLine(Language.Instance.Translations["JOB_STATE_ENDED"] + " : " + e.JobState.BackupJob.Name);
                break;
        }
    }

    public void OnConfigurationChanged(object sender, ConfigurationChangedEventArgs e) {
        Console.WriteLine(Language.Instance.Translations["CONFIGURATION_CHANGED"]);
        foreach (IBackupJobConfiguration job in this._ViewModel.Configuration.Jobs) {
            Console.WriteLine(Language.Instance.Translations["JOB_NAME"] + ": " + job.Name +
                 ", " + Language.Instance.Translations["JOB_SOURCE"] + ": " + job.Source +
                 ", " + Language.Instance.Translations["JOB_DESTINATION"] + ": " + job.Destination +
                 ", " + Language.Instance.Translations["JOB_TYPE"] + ": " + job.Type);
        }
    }
}

