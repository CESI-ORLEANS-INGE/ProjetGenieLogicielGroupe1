using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using static EasySave.IViewModel;

namespace EasySave.Model
{
    public interface IConfigurationManager
    {
        IConfigurationFile File { get; set; }
        IConfiguration Load(string path);
        void OnConfigurationChanged();
    }

    public class ConfigurationManager : IConfigurationManager
    {
        // Singleton instance of ConfigurationManager
        public static ConfigurationManager Instance { get; } = new ConfigurationManager();

        // Private constructor to prevent instantiation from outside
        private ConfigurationManager() { }

        // Properties
        public IConfigurationFile File { get; set; } = new ConfigurationFile();
        public IConfiguration configuration { get; set; } 

        public Configuration Load(string path)
        {
            // Load the configuration from the file  
            dynamic jason = File.Read(path);

            // extract the language from the configurationfile
            string language = jason.Language;
            // extract the jobs from the configurationfile
            List<IBackupJobConfiguration> jobs = jason.Jobs;
            // Initialize the configuration with the loaded values
            configuration = Configuration.Init(language, jobs);

            // Place all Subscriptions to the events here
            // Subscribe to the ConfigurationChanged event
            configuration.ConfigurationChanged += (sender, e) => OnConfigurationChanged();
            // Subscribe to the JobConfigurationChanged event
            configuration.ConfigurationChanged += (sender, e) => OnConfigurationChanged();
            // Subscribe to the LanguageChanged event
            Language.Instance.LanguageChanged += (sender, e) => OnConfigurationChanged();

            // Return the loaded configuration  
            return (Configuration)configuration;
        }

        public void OnConfigurationChanged()
        {
            // structuralize the configuration Save in json format
            //initialize the string 
            string NewConfiguration;

            // add the language to the configuration
            string language = Language.Instance.GetLanguage();
            NewConfiguration = "{[\"Language\":\"";
            NewConfiguration += language;

            // add jobs configurations to the configuration
            NewConfiguration += "\",\"Jobs\":[";
            int NumberOfJobs = 0;
            foreach (var BackupJobConfiguration in configuration.Jobs) // Fixed: Added 'var' and specified 'configuration.Jobs'
            {
                NumberOfJobs++;
                NewConfiguration += "{[\"Name\":\"";
                NewConfiguration += BackupJobConfiguration.Name;
                NewConfiguration += "\",\"Source\":\"";
                NewConfiguration += BackupJobConfiguration.Source;
                NewConfiguration += "\",\"Destination\":\"";
                NewConfiguration += BackupJobConfiguration.Destination;
                NewConfiguration += "\",\"Type\":\"";
                NewConfiguration += BackupJobConfiguration.Type;
                NewConfiguration += "\"},";
            }
            NewConfiguration += "]}";

            // parse the string to json
            JsonDocument jsonDocument = JsonDocument.Parse(NewConfiguration);

            // Convert JsonDocument to JsonArray
            JsonArray jsonArray = new JsonArray();
            foreach (var element in jsonDocument.RootElement.EnumerateArray())
            {
                jsonArray.Add(JsonNode.Parse(element.GetRawText()));
            }

            File.Save(jsonArray);
        }

        IConfiguration IConfigurationManager.Load(string path)
        {
            return Load(path);
        }
    }
}