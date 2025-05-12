using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using static EasySave.Model.IConfigurationManager;

namespace EasySave.Model {
    public interface IConfigurationFile 
    {
        // Interface for ConfigurationFile class
        /// <summary>
        /// Save the configuration to a file
        /// </summary>
        /// <param name="configuration"></param>
        void Save(JsonArray configuration);

        /// <summary>
        /// Read the configuration from a file
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
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
            // Parse the JSON string into a JsonNode
            JsonNode? jsonNode = JsonNode.Parse(json);
            // Check if the parsed JSON is a JsonArray
            if (jsonNode is JsonArray jsonArray)
            {
                // Successfully parsed as JsonArray
                return jsonArray;
            }
            else
            {
                // Handle the case where the JSON is not a JsonArray
                throw new InvalidOperationException("The JSON content is not a valid JsonArray.");
            }
        }
    }
}