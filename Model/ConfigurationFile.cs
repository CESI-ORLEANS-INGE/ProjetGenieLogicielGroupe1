using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;


namespace EasySave.Model;

public interface IConfigurationFile {
    /// <summary>
    /// Method to save the configuration to a file
    /// </summary>
    void Save(IConfiguration configuration);
    /// <summary>
    /// Method to read the configuration from a file
    /// </summary>
    IConfiguration Read();
}

public class ConfigurationJSONFile(string filePath) : IConfigurationFile {
    // set the file path
    private string FilePath { get; set; } = filePath;

    /// <summary>
    /// Save the configuration to a file
    /// </summary>
    /// param name="configuration"></param>
    /// returns></returns>
    public void Save(IConfiguration configuration) {
        var jsonObject = configuration.ToJSON();

        // put the state file in the json object
        string jsonString = jsonObject.ToJsonString(new JsonSerializerOptions {
            WriteIndented = true
        });

        // write the json object to the file
        using StreamWriter writer = new(this.FilePath);
        writer.Write(jsonString);
    }

    /// <summary>
    /// Read the configuration from a file
    /// </summary>
    /// <returns>
    /// IConfiguration
    /// </returns>
    /// <exception cref="InvalidOperationException"></exception>
    public IConfiguration Read() {
        bool isNew = false;
        // check if the file exists
        if (!File.Exists(this.FilePath)) {
            // if the file does not exist, create it
            using StreamWriter writer = new(this.FilePath);
            writer.Write("{}");
            isNew = true;
        }

        // read the file
        string json = File.ReadAllText(this.FilePath);
        JsonNode? jsonNode = JsonNode.Parse(json);

        // check if the json is valid
        if (jsonNode is JsonObject jsonObject) {
            // get the language from the json object
            string language = jsonObject["Language"]?.ToString() ?? IConfiguration.DEFAULT_LANGUAGE;
            // get the state file from the json object
            string stateFile = jsonObject["StateFile"]?.ToString() ?? IConfiguration.DEFAULT_STATE_FILE;
            string logFile = jsonObject["LogFile"]?.ToString() ?? IConfiguration.DEFAULT_LOG_FILE;
            string cryptoFile = jsonObject["CryptoFile"]?.ToString() ?? IConfiguration.DEFAULT_CRYPTO_FILE;
            string cryptoKey = jsonObject["CryptoKey"]?.ToString() ?? IConfiguration.DEFAULT_CRYPTO_KEY;
            List<string> cryptoExtentions = jsonObject["CryptoExtensions"] is JsonArray array
                ? [.. array.Select(x=>x?.ToString() ?? string.Empty)]
                : [];

            // get the jobs from the json object
            List <IBackupJobConfiguration> jobs = [];
            if (jsonObject["Jobs"] is JsonArray jobsArray) {
                for (int i = 0; i < jobsArray.Count; i++) {
                    JsonNode? job = jobsArray[i];
                    // check if the job is a json object
                    if (job is JsonObject jobObject) {
                        // add the job to the list of jobs
                        jobs.Add(new BackupJobConfiguration {
                            Name = jobObject["Name"]?.ToString() ??
                                throw new InvalidOperationException($"Missing name in configuration for job at position {i}"),
                            Source = jobObject["Source"]?.ToString() ??
                                throw new InvalidOperationException($"Missing source in configuration for job: {jobObject["Name"]?.ToString()}"),
                            Destination = jobObject["Destination"]?.ToString() ??
                                throw new InvalidOperationException($"Missing destination in configuration for job: {jobObject["Name"]?.ToString()}"),
                            Type = jobObject["Type"]?.ToString() ??
                                throw new InvalidOperationException($"Missing job type in configuration for job: {jobObject["Name"]?.ToString()}")
                        });
                    }
                }
            }

            // create the configuration
            IConfiguration configuration = new Configuration(new {
                // set the properties of the configuration
                Language = language,
                StateFile = stateFile,
                LogFile = logFile,
                CryptoFile = cryptoFile,
                CryptoKey = cryptoKey,
                CryptoExtentions = cryptoExtentions,
                Jobs = jobs
            });


            // Save the configuration if it is new
            if (isNew) {
                // Save the configuration to the file
                this.Save(configuration);
            }

            return configuration;
        // if the json is not a valid json object
        // throw new InvalidOperationException("The JSON content is not a valid JsonObject.");
        }
        else {
            // return the error
            throw new InvalidOperationException("The JSON content is not a valid JsonObject.");
        }
    }
}