using System;
using System.Runtime.ExceptionServices;
using EasySave.Model;
using Logger;

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

    // +----------------------------------+
    // |          EVENTS HANDLERS         |
    // +----------------------------------+

    /// <summary>
    /// Handles the property changed event.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed.</param>
    public static abstract void OnPropertyChanged(string propertyName);
    /// <summary>
    /// Handles the language changed event.
    /// </summary>
    /// <param name="language">The new language.</param>
    public static abstract void OnLanguageChanged(string language);
    /// <summary>
    /// Handles the job state changed event.
    /// </summary>
    /// <param name="jobState">The new job state.</param>
    public static abstract void OnJobStateChanged(IBackupJobState jobState);
    /// <summary>
    /// Handles the configuration changed event.
    /// </summary>
    public static abstract void OnConfigurationChanged();
}

public class View : IView {
    private static IViewModel? ViewModel;

    public static void Main(string[] args) {
        // Initialize the ViewModel
        View.ViewModel = new ViewModel();

        if (args.Length > 0) {
            switch (args[0].ToLower()) {
                case "list":
                    View.RunCommandList();
                    break;
                case "run":
                    View.RunCommandRun(View.ParseJobList(args[1]));
                    break;
                case "add":
                    View.RunCommandAdd(args[1], args[2], args[3], args[4]);
                    break;
                case "remove":
                    View.RunCommandRemove(args[1]);
                    break;
                case "language":
                    View.RunCommandLanguage(args[1]);
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
                Console.WriteLine("+----------------------------------------------------+");
                Console.Write("+ " + Language.Instance.Translations["MENU_CHOICE"] + ": ");
                string? choice = Console.ReadLine();
                Console.WriteLine(string.Empty);

                try {
                    if (choice == "1") {
                        View.RunCommandList();
                    } else if (choice == "2") {
                        Console.Write("+ " + Language.Instance.Translations["RUN_INPUT"] + ": ");
                        string? jobList = Console.ReadLine();
                        if (jobList is null) {
                            Console.WriteLine(Language.Instance.Translations["INVALID_INPUT"]);
                            continue;
                        }
                        View.RunCommandRun(View.ParseJobList(jobList));
                    } else if (choice == "3") {
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
                    } else if (choice == "4") {
                        Console.Write("+ " + Language.Instance.Translations["REMOVE_INPUT"] + ": ");
                        string? indexOrName = Console.ReadLine();
                        if (indexOrName is null) {
                            Console.WriteLine(Language.Instance.Translations["INVALID_INPUT"]);
                            continue;
                        }
                        View.RunCommandRemove(indexOrName);
                    } else if (choice == "5") {
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
                    } else {
                        Console.WriteLine(Language.Instance.Translations["UNKNOWN_OPTION"]);
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
                foreach (string index in indexOrName.Split('-')) {
                    if (int.TryParse(index, out int id)) {
                        if (!indexOrNameList.Contains(index)) {
                            indexOrNameList.Add(index);
                        }
                    } else {
                        throw new Exception(Language.Instance.Translations["INVALID_INPUT"] + ": " + (string)index);
                    }
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

    public static void OnPropertyChanged(string propertyName) {
        // nothing
    }

    public static void OnLanguageChanged(string language) {
        Console.WriteLine(Language.Instance.Translations["LANGUAGE_CHANGED"] + ": " + language);
    }

    public static void OnJobStateChanged(IBackupJobState jobState) {
        Console.WriteLine(Language.Instance.Translations["JOB_STATE_CHANGED"] + ": " + jobState.BackupJob.Name + ", " + Language.Instance.Translations["STATE"] + ": " + jobState.State);
    }

    public static void OnConfigurationChanged() {
        Console.WriteLine(Language.Instance.Translations["CONFIGURATION_CHANGED"]);
        foreach (IBackupJobConfiguration job in ViewModel!.Configuration.Jobs) {
            Console.WriteLine(Language.Instance.Translations["JOB_NAME"] + ": " + job.Name +
                 ", " + Language.Instance.Translations["JOB_SOURCE"] + ": " + job.Source +
                 ", " + Language.Instance.Translations["JOB_DESTINATION"] + ": " + job.Destination +
                 ", " + Language.Instance.Translations["JOB_TYPE"] + ": " + job.Type);
        }
    }
}