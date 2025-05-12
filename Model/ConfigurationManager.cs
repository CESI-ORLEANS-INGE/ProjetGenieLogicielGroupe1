using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using static EasySave.IViewModel;

namespace EasySave.Model
{
    public interface IConfigurationManager
    {
        IFile File { get; set; }
        IConfiguration Load(string path);
        void OnConfigurationChanged();
    }

    public class ConfigurationManager : IConfigurationManager
    {
        // Singleton instance of ConfigurationManager
        public static ConfigurationManager Instance { get; } = new ConfigurationManager();

        // Private constructor to prevent instantiation from outside
        private ConfigurationManager() { }

        public IFile File { get; set; } = new ConfigurationFile();
        public IConfiguration configuration { get; set; } = new Configuration();

        public Configuration Load(string path)
        {
            // Load the configuration from the file  
            configuration = File.Load(path);

            // Return the loaded configuration  
            return (Configuration)configuration;
        }

        public void OnConfigurationChanged()
        {
            // structuralize the configuration Save in json format
            //initialize the string 
            string NewConfiguration;

            // add the language to the configuration
            string language = Language.GetLanguage();
            NewConfiguration = "{[\"Language\":\"";
            NewConfiguration += language;

            // add jobs configurations to the configuration
            NewConfiguration += "\",\"Jobs\":[";
            int NumberOfJobs = 0;
            foreach (BackupJobConfiguration in Configuration.Jobs)
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