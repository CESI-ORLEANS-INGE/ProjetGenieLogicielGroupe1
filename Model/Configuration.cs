using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace EasySave.Model {
    /// <summary>
    /// Event arguments for the ConfigurationChanged event
    /// </summary>
    public class ConfigurationChangedEventArgs : EventArgs {
        // Property name that changed
        public string? PropertyName { get; set; }
    }
    // delegate for the ConfigurationChanged event
    public delegate void ConfigurationChangedEventHandler(object sender, ConfigurationChangedEventArgs e);

    /// <summary>
    /// Configuration interface
    /// </summary>
    public interface IConfiguration {
        public const string DEFAULT_LANGUAGE = "FR";
        public const string DEFAULT_STATE_FILE = "state.json";
        public const string DEFAULT_LOG_FILE = "logs.json";
        public const string DEFAULT_CRYPTO_FILE = "CryptoSoft/CryptoSoft.exe";
        public const string DEFAULT_CRYPTO_KEY = "7A2F8D15E9C3B6410D5F78A92E64B0C3DB91A527F836E45C0B2D7498C1E5A3F6";

        string Language { get; set; }
        string StateFile { get; set;  }
        string LogFile { get; set;  }
        ObservableCollection<string> Processes { get; set; }
        ObservableCollection<string> CryptoExtentions { get; set; }
        string CryptoFile { get; set; }
        string CryptoKey { get; set; }
        List<IBackupJobConfiguration> Jobs { get; set; }

        public void AddJob(IBackupJobConfiguration jobConfiguration);
        public void RemoveJob(IBackupJobConfiguration jobConfiguration);
        public JsonObject ToJSON();

        event ConfigurationChangedEventHandler ConfigurationChanged;
    }

    public class Configuration : IConfiguration {
        /// <summary>
        /// Singleton instance of Configuration
        public static Configuration? Instance { get; private set; }

        // private fields
        // Language of the application
        private static string? _Language;
        // State file of the application
        private static string? _StateFile;
        // Log file of the application
        private static string? _LogFile;
        // List of processes to detect
        private static string? _CryptoKey;
        private static ObservableCollection<string>? _Processes;
        // List of crypt extentions
        private static ObservableCollection<string>? _CryptExtentions;
        // Crypto file of the application
        private static string? _CryptoFile;
        // List of jobs
        private static List<IBackupJobConfiguration>? _Jobs;
        public List<string> CryptExt { get; set; }

        /// <summary>
        /// Language of the application
        /// </summary>
        public string Language {
            // getting the language default value
            get => _Language ?? IConfiguration.DEFAULT_LANGUAGE;
            // setting the language and raising the event
            set
            {
                // load the language
                _Language = value;
                // set the configuration changed event
                this.ConfigurationChanged?.Invoke(this, new ConfigurationChangedEventArgs {
                    // set the new language to save
                    PropertyName = nameof(Language)
                });
            }
        }
        /// <summary>
        /// state file of the application
        /// </summary>
        public string StateFile {
            // check if the state file is set
            // if not, throw an exception
            get => _StateFile ?? throw new InvalidOperationException("State file is not set");
            // set the state file and raise the event
            set
            {
                // set the state file
                _StateFile = value;
                // set the configuration changed event
                this.ConfigurationChanged?.Invoke(this, new ConfigurationChangedEventArgs {
                    // set the new state file to save
                    PropertyName = nameof(StateFile)
                });
            }
        }

        public string CryptoKey { 
            get => _CryptoKey ?? IConfiguration.DEFAULT_CRYPTO_KEY;
            set {
                // set the state file
                _CryptoKey = value;
                // set the configuration changed event
                this.ConfigurationChanged?.Invoke(this, new ConfigurationChangedEventArgs
                {
                    // set the new state file to save
                    PropertyName = nameof(CryptoKey)
                });
            } }

        public string LogFile {
            // check if the log file is set
            // if not, throw an exception
            get => _LogFile ?? throw new InvalidOperationException("Log file is not set");
            // set the log file and raise the event
            set {
                // set the log file
                _LogFile = value;
                // set the configuration changed event
                this.ConfigurationChanged?.Invoke(this, new ConfigurationChangedEventArgs {
                    // set the new log file to save
                    PropertyName = nameof(LogFile)
                });
            }
        }

        public ObservableCollection<string> Processes {
            // getting the processes list
            get => _Processes ?? new ObservableCollection<string>();
            // setting the processes list and raising the event
            set {
                _Processes = value;
                // set the configuration changed event
                this.ConfigurationChanged?.Invoke(this, new ConfigurationChangedEventArgs {
                    // set the new processes list to save
                    PropertyName = nameof(Processes)
                });
            }
        }

        public ObservableCollection<string> CryptoExtentions {
            // getting the crypt extentions list
            get => _CryptExtentions ?? new ObservableCollection<string>();
            // setting the crypt extentions list and raising the event
            set {
                _CryptExtentions = value;
                // set the configuration changed event
                this.ConfigurationChanged?.Invoke(this, new ConfigurationChangedEventArgs {
                    // set the new crypt extentions list to save
                    PropertyName = nameof(CryptoExtentions)
                });
            }
        }


        /// <summary>
        /// List of jobs
        /// </summary>
        public List<IBackupJobConfiguration> Jobs {
            // getting the jobs list
            get => _Jobs ?? [];
            // setting the jobs list and raising the event
            set
            {
                _Jobs = value;
                // set the configuration changed event
                this.ConfigurationChanged?.Invoke(this, new ConfigurationChangedEventArgs {
                    // set the new jobs list to save
                    PropertyName = nameof(Jobs)
                });
            }
        }

        /// <summary>
        /// Method to create a new instance of Configuration
        public Configuration() {
            // check if the instance is already created
            if (Configuration.Instance != null) {
                // if the instance is already created, throw an exception
                throw new InvalidOperationException("Configuration is already initialized.");
            }

            this.Processes.CollectionChanged += (sender, args) => {
                // raise the ConfigurationChanged event when a process is added or removed
                this.ConfigurationChanged?.Invoke(this, new ConfigurationChangedEventArgs {
                    // set the property name to Processes
                    PropertyName = nameof(Processes)
                });
            };

            this.CryptoExtentions.CollectionChanged += (sender, args) => {
                // raise the ConfigurationChanged event when a crypt extention is added or removed
                this.ConfigurationChanged?.Invoke(this, new ConfigurationChangedEventArgs {
                    // set the property name to CryptExtentions
                    PropertyName = nameof(CryptoExtentions)
                });
            };

            // assign of the values configuration
            Configuration.Instance = this;
        }

        public string CryptoFile {
            // check if the crypto file is set
            // if not, throw an exception
            get => _CryptoFile ?? throw new InvalidOperationException("Crypto file is not set");
            // set the crypto file and raise the event
            set {
                // set the crypto file
                _CryptoFile = value;
                // set the configuration changed event
                this.ConfigurationChanged?.Invoke(this, new ConfigurationChangedEventArgs {
                    // set the new crypto file to save
                    PropertyName = nameof(CryptoFile)
                });
            }
        }

        /// <summary>
        /// Constructor to create a new instance of Configuration
        /// </summary>
        public Configuration(object configuration) {
            // check if the instance is already initialized
            if (Configuration.Instance != null) {
                // if the instance is already created, throw an exception
                throw new InvalidOperationException("Configuration is already initialized.");
            }
            // assign the values configuration
            if (configuration.GetType().GetProperty("Language") is not null) {
                // Set the language from the configuration
                this.Language = configuration.GetType().GetProperty("Language")?.GetValue(configuration)?.ToString() ?? IConfiguration.DEFAULT_LANGUAGE;
            }
            // Check if the state file is set
            if (configuration.GetType().GetProperty("StateFile") is not null) {
                // Set the state file from the configuration
                this.StateFile = configuration.GetType().GetProperty("StateFile")?.GetValue(configuration)?.ToString() ?? IConfiguration.DEFAULT_STATE_FILE;
            }
            // Check if the log file is set
            if (configuration.GetType().GetProperty("LogFile") is not null) {
                this.LogFile = configuration.GetType().GetProperty("LogFile")?.GetValue(configuration)?.ToString() ?? IConfiguration.DEFAULT_LOG_FILE;
            }
            // Check if the crypto file is set
            if (configuration.GetType().GetProperty("CryptoFile") is not null) {
                this.CryptoFile = configuration.GetType().GetProperty("CryptoFile")?.GetValue(configuration)?.ToString() ?? IConfiguration.DEFAULT_CRYPTO_FILE;
            }
            if (configuration.GetType().GetProperty("CryptoKey") is not null)
            {
                this.CryptoKey = configuration.GetType().GetProperty("CryptoKey")?.GetValue(configuration)?.ToString() ?? IConfiguration.DEFAULT_CRYPTO_KEY;
            }
            // Check if the crypto extentions are set
            if (configuration.GetType().GetProperty("CryptoExtentions") is not null) {
                // Set the crypto extentions from the configuration
                this.CryptoExtentions = configuration.GetType().GetProperty("CryptoExtentions")?.GetValue(configuration) as ObservableCollection<string> ?? new ObservableCollection<string>();
            } else {
                // If the crypto extentions are not set, initialize them to an empty list
                this.CryptoExtentions = new ObservableCollection<string>();
            }
            // Check if the processes are set
            if (configuration.GetType().GetProperty("Processes") is not null) {
                // Set the processes from the configuration
                this.Processes = configuration.GetType().GetProperty("Processes")?.GetValue(configuration) as ObservableCollection<string> ?? new ObservableCollection<string>();
            } else {
                // If the processes are not set, initialize them to an empty list
                this.Processes = new ObservableCollection<string>();
            }
            if (configuration.GetType().GetProperty("Jobs") is not null) {
                // Set the jobs list from the configuration
                this.Jobs = configuration.GetType().GetProperty("Jobs")?.GetValue(configuration) as List<IBackupJobConfiguration> ?? [];
            } else {
                // If the jobs list is not set, initialize it to an empty list
                this.Jobs = [];
            }
            // Subscribe to the JobConfigurationChanged event for each job
            foreach (IBackupJobConfiguration jobConfiguration in this.Jobs) {
                // Subscribe to the JobConfigurationChanged event
                jobConfiguration.JobConfigurationChanged += (sender, args) => {
                    // Raise the ConfigurationChanged event when a job configuration changes
                    this.ConfigurationChanged?.Invoke(this, new ConfigurationChangedEventArgs {
                        // Set the property name to Jobs
                        PropertyName = nameof(Jobs)
                    });
                };
            }

            Configuration.Instance = this;
        }

        /// <summary>
        /// add a new job to the configuration
        /// </summary>
        /// param name="jobConfiguration"></param>
        public void AddJob(IBackupJobConfiguration jobConfiguration) {
            // add the job to the list of jobs
            this.Jobs.Add(jobConfiguration);

            // subscribe to the JobConfigurationChanged event
            jobConfiguration.JobConfigurationChanged += (sender, args) => {
                // raise the ConfigurationChanged event when a job configuration changes
                this.ConfigurationChanged?.Invoke(this, new ConfigurationChangedEventArgs {
                    // set the property name to Jobs
                    PropertyName = nameof(Jobs)
                });
            };

            // raise the ConfigurationChanged event
            this.ConfigurationChanged?.Invoke(this, new ConfigurationChangedEventArgs {
                // set the property Jobs to save
                PropertyName = nameof(Jobs)
            });
        }

        /// <summary>
        /// remove a job from the configuration
        /// </summary>
        /// param name="jobConfiguration"></param>
        public void RemoveJob(IBackupJobConfiguration jobConfiguration) {
            // remove the job from the list of jobs
            this.Jobs.Remove(jobConfiguration);
            // send the event
            // raise the ConfigurationChanged event
            this.ConfigurationChanged?.Invoke(this, new ConfigurationChangedEventArgs {
                // set the property Jobs to save
                PropertyName = nameof(Jobs)
            });
        }

        public event ConfigurationChangedEventHandler? ConfigurationChanged;

        public JsonObject ToJSON() {
            return new JsonObject {
                ["Language"] = this.Language,
                ["StateFile"] = this.StateFile,
                ["LogFile"] = this.LogFile,
                ["CryptoFile"] = this.CryptoFile,
                ["CryptoKey"] = this.CryptoKey,
                ["Processes"] = new JsonArray([.. this.Processes]),
                ["CryptoExtentions"] = new JsonArray([.. this.CryptoExtentions]),
                ["Jobs"] = new JsonArray([.. this.Jobs.Select(j => new JsonObject {
                    ["Name"] = j.Name,
                    ["Source"] = j.Source,
                    ["Destination"] = j.Destination,
                    ["Type"] = j.Type
                })])
            };
        }
    }

}