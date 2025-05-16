using System;
using System.Runtime.ExceptionServices;
using EasySave.Model;
using System.ComponentModel;
using System.Text.Json.Nodes;
using System.Text.Json;

namespace EasySave;

public interface IView {
    // +----------------------------------+
    // |            PROPERTIES            |
    // +----------------------------------+

    /// <summary>
    /// The ViewModel used in the application.
    /// </summary>
    protected static IViewModel? ViewModel { get; set; }

    // +----------------------------------+
    // |              METHODS             |
    // +----------------------------------+

    /// <summary>
    /// The entry point of the application.
    /// </summary>
    /// <param name="args">The command line arguments.</param>
    public static abstract void Main(string[] args);

    /// <summary>
    /// Runs the command to start one or more backup jobs.
    /// </summary>
    /// <param name="indexOrNameList">A list of indices or names of the backup jobs to run.</param>
    public static abstract void RunCommandRun(List<string> indexOrNameList);
    /// <summary>
    /// Runs the command to list all backup jobs.
    /// </summary>
    public static abstract void RunCommandList();
    /// <summary>
    /// Runs the command to add a new backup job.
    /// </summary>
    /// <param name="name">The name of the backup job.</param>
    /// <param name="source">The source path of the backup job.</param>
    /// <param name="destination">The destination path of the backup job.</param>
    /// <param name="type">The type of the backup job.</param>
    public static abstract void RunCommandAdd(string name, string source, string destination, string type);
    /// <summary>
    /// Runs the command to remove a backup job.
    /// </summary>
    /// <param name="indexOrName">The index or name of the backup job to remove.</param>
    public static abstract void RunCommandRemove(string indexOrName);
    /// <summary>
    /// Runs the command to change the application language.
    /// </summary>
    /// <param name="language">The language to set.</param>
    public static abstract void RunCommandLanguage(string language);
    /// <summary>
    /// Runs the command to display the configuration as JSON.
    /// </summary>
    public static abstract void RunCommandConfiguration();
    /// <summary>
    /// Runs the command to change the log file path, so that change the log file type.
    /// </summary>
    /// <param name="filePath"></param>
    public static abstract void RunCommandLog(string filePath);

    // +----------------------------------+
    // |          EVENTS HANDLERS         |
    // +----------------------------------+

    /// <summary>
    /// Handles the property changed event.
    /// </summary>
    public static abstract void OnPropertyChanged(object sender, PropertyChangedEventArgs e);
    /// <summary>
    /// Handles the language changed event.
    /// </summary>
    public static abstract void OnLanguageChanged(object sender, LanguageChangedEventArgs e);
    /// <summary>
    /// Handles the job state changed event.
    /// </summary>
    public static abstract void OnJobStateChanged(object sender, JobStateChangedEventArgs e);
    /// <summary>
    /// Handles the configuration changed event.
    /// </summary>
    public static abstract void OnConfigurationChanged(object sender, ConfigurationChangedEventArgs e);
}

public class View : IView {
    private static IViewModel? ViewModel;

    public static void Main(string[] args) {
        // Initialize the ViewModel
        View.ViewModel = new ViewModel();
        View.ViewModel.JobStateChanged += OnJobStateChanged;
        View.ViewModel.ConfigurationChanged += OnConfigurationChanged;
        View.ViewModel.LanguageChanged += OnLanguageChanged;
        View.ViewModel.PropertyChanged += OnPropertyChanged;

        if (args.Length > 0) {
            switch (args[0].ToLower()) {
                case "list":
                    View.RunCommandList();
                    break;
                case "run":
                    if (args.Length < 2) {
                        Console.WriteLine(Language.Instance.Translations["INVALID_INPUT"]);
                        return;
                    }
                    View.RunCommandRun(View.ParseJobList(args[1]));
                    break;
                case "add":
                    if (args.Length < 5) {
                        Console.WriteLine(Language.Instance.Translations["INVALID_INPUT"]);
                        return;
                    }
                    View.RunCommandAdd(args[1], args[2], args[3], args[4]);
                    break;
                case "remove":
                    if (args.Length < 2) {
                        Console.WriteLine(Language.Instance.Translations["INVALID_INPUT"]);
                        return;
                    }
                    View.RunCommandRemove(args[1]);
                    break;
                case "language":
                    if (args.Length < 2) {
                        Console.WriteLine(Language.Instance.Translations["INVALID_INPUT"]);
                        return;
                    }
                    View.RunCommandLanguage(args[1]);
                    break;
                case "configuration":
                case "config":
                    View.RunCommandConfiguration();
                    break;
                case "log":
                    if (args.Length < 2) {
                        Console.WriteLine(Language.Instance.Translations["INVALID_INPUT"]);
                        return;
                    }
                    View.RunCommandLog(args[1]);
                    break;
                default:
                    Console.WriteLine(Language.Instance.Translations["UNKNOWN_COMMAND"]);
                    break;
            }
        } else {
            while (true) {
                Console.WriteLine("+----------------------------------------------------+");
                Console.WriteLine("|                 EasySave Application               |");
                Console.WriteLine("+----------------------------------------------------+");
                Console.WriteLine("| 1. " + Language.Instance.Translations["MENU_OPTION_LIST"].PadRight(48) + '|');
                Console.WriteLine("| 2. " + Language.Instance.Translations["MENU_OPTION_RUN"].PadRight(48) + '|');
                Console.WriteLine("| 3. " + Language.Instance.Translations["MENU_OPTION_ADD"].PadRight(48) + '|');
                Console.WriteLine("| 4. " + Language.Instance.Translations["MENU_OPTION_REMOVE"].PadRight(48) + '|');
                Console.WriteLine("| 5. " + Language.Instance.Translations["MENU_OPTION_LANGUAGE"].PadRight(48) + '|');
                Console.WriteLine("| 6. " + Language.Instance.Translations["MENU_OPTION_CONFIGURATION"].PadRight(48) + '|');
                Console.WriteLine("| 7. " + Language.Instance.Translations["MENU_OPTION_LOG"].PadRight(48) + '|');
                Console.WriteLine("+----------------------------------------------------+");
                Console.Write("+ " + Language.Instance.Translations["MENU_CHOICE"] + ": ");
                string? choice = Console.ReadLine();
                Console.WriteLine(string.Empty);

                try {
                    switch (choice) {
                        case "1":
                            View.RunCommandList();
                            break;
                        case "2":
                            Console.Write("+ " + Language.Instance.Translations["RUN_INPUT"] + ": ");
                            string? jobList = Console.ReadLine();
                            if (jobList is null) {
                                Console.WriteLine(Language.Instance.Translations["INVALID_INPUT"]);
                                continue;
                            }
                            View.RunCommandRun(View.ParseJobList(jobList));
                            break;
                        case "3":
                            Console.Write("+ " + Language.Instance.Translations["ADD_NAME_INPUT"] + ": ");
                            string? name = Console.ReadLine();
                            if (name is null) {
                                Console.WriteLine(Language.Instance.Translations["INVALID_INPUT"]);
                                continue;
                            }
                            Console.Write("+ " + Language.Instance.Translations["ADD_SOURCE_INPUT"] + ": ");
                            string? source = Console.ReadLine();
                            if (source is null) {
                                Console.WriteLine(Language.Instance.Translations["INVALID_INPUT"]);
                                continue;
                            }
                            Console.Write("+ " + Language.Instance.Translations["ADD_DESTINATION_INPUT"] + ": ");
                            string? destination = Console.ReadLine();
                            if (destination is null) {
                                Console.WriteLine(Language.Instance.Translations["INVALID_INPUT"]);
                                continue;
                            }
                            Console.Write("+ " + Language.Instance.Translations["ADD_TYPE_INPUT"] + ": ");
                            string? type = Console.ReadLine();
                            if (type is null) {
                                Console.WriteLine(Language.Instance.Translations["INVALID_INPUT"]);
                                continue;
                            }
                            View.RunCommandAdd(name, source, destination, type);
                            break;
                        case "4":
                            Console.Write("+ " + Language.Instance.Translations["REMOVE_INPUT"] + ": ");
                            string? indexOrName = Console.ReadLine();
                            if (indexOrName is null) {
                                Console.WriteLine(Language.Instance.Translations["INVALID_INPUT"]);
                                continue;
                            }
                            View.RunCommandRemove(indexOrName);
                            break;
                        case "5":
                            List<string> availableLanguages = Language.Instance.GetAvailableLanguages();
                            Console.Write(
                                "+ " +
                                Language.Instance.Translations["LANGUAGE_INPUT"] +
                                " (" +
                                availableLanguages.Aggregate((a, b) => a + ", " + b) +
                                "): "
                            );
                            string? language = Console.ReadLine();
                            if (language is null) {
                                Console.WriteLine(Language.Instance.Translations["INVALID_INPUT"]);
                                continue;
                            }
                            if (!availableLanguages.Contains(language)) {
                                Console.WriteLine(Language.Instance.Translations["LANGUAGE_NOT_FOUND"]);
                                continue;
                            }
                            View.RunCommandLanguage(language);
                            break;
                        case "6":
                            View.RunCommandConfiguration();
                            break;
                        case "7":
                            Console.Write("+ " + Language.Instance.Translations["LOG_INPUT"] + ": ");
                            string? filePath = Console.ReadLine();
                            if (filePath is null) {
                                Console.WriteLine(Language.Instance.Translations["INVALID_INPUT"]);
                                continue;
                            }
                            View.RunCommandLog(filePath);
                            break;
                        default:
                            Console.WriteLine(Language.Instance.Translations["UNKNOWN_OPTION"]);
                            break;
                    }
                } catch (Exception e) {
                    Console.WriteLine(Language.Instance.Translations["ERROR_OCCURRED"] + ": " + e.Message);
                }

                Console.WriteLine(string.Empty);
            }
        }
    }

    public static List<string> ParseJobList(string jobList) {
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

    public static void RunCommandRun(List<string> indexOrNameList) {
        ViewModel?.RunCommandRun(indexOrNameList);
    }

    public static void RunCommandList() {
        if (View.ViewModel is null) {
            throw new Exception("ViewModel is not initialized.");
        }
        for (int i = 0; i < View.ViewModel.Configuration.Jobs.Count; i++) {
            IBackupJobConfiguration job = View.ViewModel.Configuration.Jobs[i];
            string prefix = new(' ', ((string.Empty + (i + 1))).Length);
            Console.WriteLine(i + 1 + ". " + Language.Instance.Translations["JOB_NAME"] + ": " + job.Name);
            Console.WriteLine(prefix + ". " + Language.Instance.Translations["JOB_SOURCE"] + ": " + job.Source);
            Console.WriteLine(prefix + ". " + Language.Instance.Translations["JOB_DESTINATION"] + ": " + job.Destination);
            Console.WriteLine(prefix + ". " + Language.Instance.Translations["JOB_TYPE"] + ": " + job.Type);
            Console.WriteLine(string.Empty);
        }
    }

    public static void RunCommandAdd(string name, string source, string destination, string type) {
        ViewModel?.RunCommandAdd(name, source, destination, type);
    }

    public static void RunCommandRemove(string indexOrName) {
        ViewModel?.RunCommandRemove(indexOrName);
    }

    public static void RunCommandLanguage(string language) {
        ViewModel?.RunCommandLanguage(language);
    }

    public static void RunCommandConfiguration() {
        var jsonObject = Configuration.Instance?.ToJSON() ?? new JsonObject();

        string jsonString = jsonObject.ToJsonString(new JsonSerializerOptions {
            WriteIndented = true
        });

        Console.WriteLine(jsonString);
    }

    public static void RunCommandLog(string filePath) {
        ViewModel?.RunCommandLog(filePath);
    }


    public static void OnPropertyChanged(object sender, PropertyChangedEventArgs e) {
        // nothing
    }

    public static void OnLanguageChanged(object sender, LanguageChangedEventArgs e) {
        Console.WriteLine(Language.Instance.Translations["LANGUAGE_CHANGED"] + ": " + e.Language);
    }

    public static void OnJobStateChanged(object sender, JobStateChangedEventArgs e) {
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

    public static void OnConfigurationChanged(object sender, ConfigurationChangedEventArgs e) {
        Console.WriteLine(Language.Instance.Translations["CONFIGURATION_CHANGED"]);
        foreach (IBackupJobConfiguration job in ViewModel!.Configuration.Jobs) {
            Console.WriteLine(Language.Instance.Translations["JOB_NAME"] + ": " + job.Name +
                 ", " + Language.Instance.Translations["JOB_SOURCE"] + ": " + job.Source +
                 ", " + Language.Instance.Translations["JOB_DESTINATION"] + ": " + job.Destination +
                 ", " + Language.Instance.Translations["JOB_TYPE"] + ": " + job.Type);
        }
    }
}