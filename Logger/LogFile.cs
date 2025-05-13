using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace EasySave.Logger {
    // Interface defining methods for managing log files
    public interface ILogFile {
        // Method to save a log entry to a file
        public void Save(Log log, string filepath);

        // Method to read logs from a file
        public List<Log> Read(string filepath);
    }

    // Implementation of the ILogFile interface
    public class LogFile : ILogFile {
        // Saves a Log object to a JSON file
        public void Save(Log log, string filePath) {
            // Create a JSON object from the log properties
            var jsonObject = new JsonObject {
                ["DateTime"] = log.Datetime,
                ["Name"] = log.JobName,
                ["Destination"] = log.Destination,
                ["Source"] = log.Source,
                ["TaskType"] = log.TaskType,
                ["Filesize"] = log.Filesize,
                ["TransfertDuration"] = log.TransfertDuration,
                ["Level"] = log.Level.ToString(),
            };

            // Convert the JSON object to a string
            string jsonString = jsonObject.ToJsonString();

            using StreamWriter file = File.AppendText(filePath); // Open the file for appending
            file.WriteLine(jsonString); // Write the JSON string to the file
        }

        // Reads the content of a JSON file and returns a list of Log objects
        public List<Log> Read(string filePath) {
            // Check if the file exists
            if (!File.Exists(filePath)) {
                Console.WriteLine("JSON file not found.");
                return new List<Log>();
            }

            try {
                // Read the entire file content
                string jsonContent = File.ReadAllText(filePath);

                // Return an empty list if the file is empty or contains only whitespace
                if (string.IsNullOrWhiteSpace(jsonContent)) {
                    return new List<Log>();
                }

                // Try to deserialize the JSON content into a list of Log objects
                var transfers = JsonSerializer.Deserialize<List<Log>>(jsonContent);

                // Return the deserialized list, or an empty list if null
                return transfers ?? new List<Log>();
            } catch (Exception ex) {
                // Handle errors during file reading or deserialization
                Console.WriteLine($"Error reading JSON file: {ex.Message}");
                return new List<Log>();
            }
        }
    }
}
