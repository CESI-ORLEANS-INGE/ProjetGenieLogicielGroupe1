using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace EasySave.Model;

public interface IConfigurationFile {
    void Save(IConfiguration configuration);
    IConfiguration Read();
}

public class ConfigurationJSONFile(string filePath) : IConfigurationFile {
    private string FilePath { get; set; } = filePath;

    public void Save(IConfiguration configuration) {
        var jsonObject = configuration.ToJSON();

        string jsonString = jsonObject.ToJsonString(new JsonSerializerOptions {
            WriteIndented = true
        });

        using StreamWriter writer = new(this.FilePath);
        writer.Write(jsonString);
    }

    public IConfiguration Read() {
        bool isNew = false;
        if (!File.Exists(this.FilePath)) {
            using StreamWriter writer = new(this.FilePath);
            writer.Write("{}");
            isNew = true;
        }

        string json = File.ReadAllText(this.FilePath);
        JsonNode? jsonNode = JsonNode.Parse(json);

        if (jsonNode is JsonObject jsonObject) {
            string language = jsonObject["Language"]?.ToString() ?? IConfiguration.DEFAULT_LANGUAGE;
            string stateFile = jsonObject["StateFile"]?.ToString() ?? IConfiguration.DEFAULT_STATE_FILE;
            string logFile = jsonObject["LogFile"]?.ToString() ?? IConfiguration.DEFAULT_LOG_FILE;

            List<IBackupJobConfiguration> jobs = [];
            if (jsonObject["Jobs"] is JsonArray jobsArray) {
                for (int i = 0; i < jobsArray.Count; i++) {
                    JsonNode? job = jobsArray[i];
                    if (job is JsonObject jobObject) {
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

            IConfiguration configuration = new Configuration(new {
                Language = language,
                StateFile = stateFile,
                LogFile = logFile,
                Jobs = jobs
            });

            if (isNew) {
                this.Save(configuration);
            }

            return configuration;
        } else {
            throw new InvalidOperationException("The JSON content is not a valid JsonObject.");
        }
    }
}