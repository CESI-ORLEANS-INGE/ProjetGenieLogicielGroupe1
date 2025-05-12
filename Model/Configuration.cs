using System;
using System.Collections.Generic;

using static EasySave.Model.IConfigurationManager;

namespace EasySave.Model {
    public interface IConfiguration {
        // Interface for Configuration class
        /// <summary>
        /// Singleton instance of Configuration
        /// </summary>
        string Language { get; set; }
        /// <summary>
        /// List of backup jobs
        /// </summary>
        List<IBackupJobConfiguration> Jobs { get; set; }
        /// <summary>
        /// Event triggered when the configuration changes
        /// </summary>
        void OnJobConfigurationChanged();
        /// <summary>
        /// Event triggered when the configuration changes
        /// </summary>
        event EventHandler ConfigurationChanged;
    }

    public class Configuration : IConfiguration
    {
        // Singleton instance of Configuration
        public static Configuration Instance { get; } = new Configuration();

        // Private constructor to prevent instantiation from outside
        private Configuration() { } 

        public static Configuration Init(string language , List<IBackupJobConfiguration> Jobs)
        {
            //Verify if the configuration is already initialized
            if (Instance != null)
            {
                throw new InvalidOperationException("Configuration is already initialized.");
            }
            // Initialize the configuration with the provided language and jobs
            Configuration configuration = new Configuration();
            configuration.Language = language;
            configuration.Jobs = Jobs;
            return configuration;
        }

        // Properties
        public string Language { get; set; } = string.Empty; 
        public List<IBackupJobConfiguration> Jobs { get; set; }
        public event EventHandler ConfigurationChanged = delegate { }; 
        
        public void OnJobConfigurationChanged()
        {
            ConfigurationChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}