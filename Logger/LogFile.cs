using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Linq;

namespace EasySave.Logger {
    // Interface defining methods for managing log files
    public interface ILogFile {
        // Method to save a log entry to a file
        public void Save(Log log, string filepath);

        // Method to read logs from a file
        public List<Log> Read(string filepath);
    }

    // Implementation of the ILogFile interface
    public class LogFileXML : ILogFile {
        // Saves a Log object to a XML file
        public void Save(Log log, string filePath) {
            XDocument xml;
            // Si le fichier n'existe pas, on le crée avec une racine <Logs>
            if (!File.Exists(filePath))
            {
                xml = new XDocument(new XElement("Logs"));
            }
            else
            {
                xml = XDocument.Load(filePath);
            }

            // Create a JSON object from the log properties
            XElement newLog = new XElement("log", 
                new XElement("DateTime",log.Datetime),
                new XElement("Name", log.JobName),
                new XElement("Destination", log.Destination),
                new XElement("Source", log.Source),
                new XElement("TaskType", log.TaskType),
                new XElement("Filesize", log.Filesize),
                new XElement("TransfertDuration", log.TransfertDuration),
                new XElement("Level", log.Level.ToString())
                );


            xml.Root.Add(newLog);
            xml.Save(filePath);
            Console.WriteLine("Fichier XML modifié avec succès.");
        }
        // Reads the content of a JSON file and returns a list of Log objects
        

        public List<Log> Read(string filePath)
            {
                // Vérifie si le fichier existe
                if (!File.Exists(filePath))
                {
                    Console.WriteLine("XML file not found.");
                    return new List<Log>();
                }

                try
                {
                    // Charge le document XML
                    XDocument doc = XDocument.Load(filePath);

                    // Liste pour stocker les logs
                    List<Log> logs = new List<Log>();

                    // Parcourt chaque élément <Log>
                    foreach (XElement logElement in doc.Descendants("Log"))
                    {
                        Log log = new Log
                        {
                            Datetime = DateTime.Parse(logElement.Element("DateTime")?.Value ?? DateTime.MinValue.ToString()),
                            JobName = logElement.Element("JobName")?.Value ?? "",
                            Source = logElement.Element("Source")?.Value ?? "",
                            Destination = logElement.Element("Destination")?.Value ?? "",
                            TaskType = logElement.Element("TaskType")?.Value ?? "",
                            Filesize = double.TryParse(logElement.Element("Filesize")?.Value, out double size) ? size : 0,
                            TransfertDuration = double.TryParse(logElement.Element("TransfertDuration")?.Value, out double dur) ? dur : 0,
                            Level = Enum.TryParse<LogLevel>(logElement.Element("Level")?.Value, out var level) ? level : LogLevel.Information,
                        };

                        logs.Add(log);
                    }

                    return logs;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading XML file: {ex.Message}");
                    return new List<Log>();
                }
            }

}
    public class LogFileJSON : ILogFile
    { // Saves a Log object to a JSON file
        public void Save(Log log, string filePath)
        {

            // Create a JSON object from the log properties
            var jsonObject = new JsonObject
            {
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
        public List<Log> Read(string filePath)
        {
            // Check if the file exists
            if (!File.Exists(filePath))
            {
                Console.WriteLine("JSON file not found.");
                return new List<Log>();
            }

            try
            {
                // Read the entire file content
                string jsonContent = File.ReadAllText(filePath);

                // Return an empty list if the file is empty or contains only whitespace
                if (string.IsNullOrWhiteSpace(jsonContent))
                {
                    return new List<Log>();
                }

                // Try to deserialize the JSON content into a list of Log objects
                var transfers = JsonSerializer.Deserialize<List<Log>>(jsonContent);

                // Return the deserialized list, or an empty list if null
                return transfers ?? new List<Log>();
            }
            catch (Exception ex)
            {
                // Handle errors during file reading or deserialization
                Console.WriteLine($"Error reading JSON file: {ex.Message}");
                return new List<Log>();
            }
        }
    }
}
