using System;
using System.Collections.Generic;

using static EasySave.Model.IConfigurationManager;

namespace EasySave.Model {
    public interface IConfiguration {
        string Language { get; set; }
        List<IBackupJobConfiguration> Jobs { get; set; }
        void OnJobConfigurationChanged();
        event EventHandler ConfigurationChanged;
    }

    public class Configuration : IConfiguration
    {
        // Singleton instance of Configuration
        public static Configuration Instance { get; } = new Configuration();

        // Private constructor to prevent instantiation from outside
        private Configuration() { }

        // Properties
        public string Language { get; set; } = string.Empty; 
        public List<IBackupJobConfiguration> Jobs { get; set; }
        public event EventHandler ConfigurationChanged = delegate { }; 
        public Configuration()
        {
            Jobs = new List<IBackupJobConfiguration>();
        }
        public void OnJobConfigurationChanged()
        {
            ConfigurationChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}