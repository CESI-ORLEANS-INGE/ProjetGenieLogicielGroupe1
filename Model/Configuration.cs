using System;
using System.Collections.Generic;

namespace EasySave.Model {
    public class ConfigurationChangedEventArgs : EventArgs {
        public string? PropertyName { get; set; }
    }
    public delegate void ConfigurationChangedEventHandler(object sender, ConfigurationChangedEventArgs e);

    public interface IConfiguration {
        public const string DEFAULT_LANGUAGE = "FR";
        public const string DEFAULT_STATE_FILE = "state.json";
        public const string DEFAULT_LOG_FILE = "logs.json";

        string Language { get; set; }
        string StateFile { get; }
        string LogFile { get; set; }
        List<IBackupJobConfiguration> Jobs { get; set; }

        public void AddJob(IBackupJobConfiguration jobConfiguration);
        public void RemoveJob(IBackupJobConfiguration jobConfiguration);

        event ConfigurationChangedEventHandler ConfigurationChanged;
    }

    public class Configuration : IConfiguration {
        public static Configuration? Instance { get; private set; }

        private static string? _Language;
        private static string? _StateFile;
        private static string? _LogFile;
        private static List<IBackupJobConfiguration>? _Jobs;

        public string Language {
            get => _Language ?? IConfiguration.DEFAULT_LANGUAGE;
            set {
                _Language = value;
                this.ConfigurationChanged?.Invoke(this, new ConfigurationChangedEventArgs {
                    PropertyName = nameof(Language)
                });
            }
        }
        public string StateFile {
            get => _StateFile ?? throw new InvalidOperationException("State file is not set");
            set {
                _StateFile = value;
                this.ConfigurationChanged?.Invoke(this, new ConfigurationChangedEventArgs {
                    PropertyName = nameof(StateFile)
                });
            }
        }
        public string LogFile {
            get => _LogFile ?? IConfiguration.DEFAULT_LOG_FILE;
            set {
                _LogFile = value;
                this.ConfigurationChanged?.Invoke(this, new ConfigurationChangedEventArgs {
                    PropertyName = nameof(LogFile)
                });
            }
        }
        public List<IBackupJobConfiguration> Jobs {
            get => _Jobs ?? [];
            set {
                _Jobs = value;
                this.ConfigurationChanged?.Invoke(this, new ConfigurationChangedEventArgs {
                    PropertyName = nameof(Jobs)
                });
            }
        }

        public Configuration() {
            if (Configuration.Instance != null) {
                throw new InvalidOperationException("Configuration is already initialized.");
            }

            Configuration.Instance = this;
        }

        public Configuration(object configuration) {
            if (Configuration.Instance != null) {
                throw new InvalidOperationException("Configuration is already initialized.");
            }

            if (configuration.GetType().GetProperty("Language") is not null) {
                this.Language = configuration.GetType().GetProperty("Language")?.GetValue(configuration)?.ToString() ?? IConfiguration.DEFAULT_LANGUAGE;
            }
            if (configuration.GetType().GetProperty("StateFile") is not null) {
                this.StateFile = configuration.GetType().GetProperty("StateFile")?.GetValue(configuration)?.ToString() ?? IConfiguration.DEFAULT_STATE_FILE;
            }
            if (configuration.GetType().GetProperty("LogFile") is not null) {
                this.LogFile = configuration.GetType().GetProperty("LogFile")?.GetValue(configuration)?.ToString() ?? IConfiguration.DEFAULT_LOG_FILE;
            }
            if (configuration.GetType().GetProperty("Jobs") is not null) {
                this.Jobs = configuration.GetType().GetProperty("Jobs")?.GetValue(configuration) as List<IBackupJobConfiguration> ?? [];
            } else {
                this.Jobs = [];
            }

            foreach (IBackupJobConfiguration jobConfiguration in this.Jobs) {
                jobConfiguration.JobConfigurationChanged += (sender, args) => {
                    this.ConfigurationChanged?.Invoke(this, new ConfigurationChangedEventArgs {
                        PropertyName = nameof(Jobs)
                    });
                };
            }

            Configuration.Instance = this;
        }

        public void AddJob(IBackupJobConfiguration jobConfiguration) {
            this.Jobs.Add(jobConfiguration);

            jobConfiguration.JobConfigurationChanged += (sender, args) => {
                this.ConfigurationChanged?.Invoke(this, new ConfigurationChangedEventArgs {
                    PropertyName = nameof(Jobs)
                });
            };

            this.ConfigurationChanged?.Invoke(this, new ConfigurationChangedEventArgs {
                PropertyName = nameof(Jobs)
            });
        }

        public void RemoveJob(IBackupJobConfiguration jobConfiguration) {
            this.Jobs.Remove(jobConfiguration);
            this.ConfigurationChanged?.Invoke(this, new ConfigurationChangedEventArgs {
                PropertyName = nameof(Jobs)
            });
        }

        public event ConfigurationChangedEventHandler? ConfigurationChanged;
    }
}