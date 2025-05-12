using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using static EasySave.Model.IConfigurationManager;

namespace EasySave.Model {
    public interface IConfigurationFile 
    {
        void Save(JsonArray configuration);
        JsonArray Read(string path);
    }

    public class ConfigurationFile : IConfigurationFile
    {
        public void Save(JsonArray configuration)
        {

            // Convert JsonArray to JsonDocument
            JsonDocument jsonDocument = JsonDocument.Parse(configuration.ToString());
            // save the json document to a file
            string jsonString = jsonDocument.RootElement.ToString();
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EasySave", "configuration.json");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, jsonString);
            // Display a message indicating that the configuration has been saved
            Console.WriteLine("Configuration saved.");
        }

        public JsonArray Read(string path)
        {
            Console.WriteLine($"Reading configuration from {path}");
            string json = File.ReadAllText(path);
            JsonNode? jsonNode = JsonNode.Parse(json);

            if (jsonNode is JsonArray jsonArray)
            {
                return jsonArray;
            }
            else
            {
                throw new InvalidOperationException("The JSON content is not a valid JsonArray.");
            }
        }
    }
}