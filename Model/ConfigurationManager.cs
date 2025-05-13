using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace EasySave.Model;

public interface IConfigurationManager {
    static IConfigurationManager? Instance { get; }
    IConfiguration Load(string filePath);
    void Save(string filePath, IConfiguration configuration);
}

public class ConfigurationManager : IConfigurationManager {
    public static ConfigurationManager? Instance { get; private set; }
    private readonly Type Loader;
    public IConfiguration? Configuration { get; set; }

    public ConfigurationManager(Type loader) {
        if (ConfigurationManager.Instance != null) {
            throw new InvalidOperationException("ConfigurationManager is a singleton. Use Instance property to access it.");
        }

        if (!typeof(IConfigurationFile).IsAssignableFrom(loader)){
            throw new ArgumentException("Loader must implement IConfigurationFile", nameof(loader));
        }

        this.Loader = loader;

        ConfigurationManager.Instance = this;
    }


    public IConfiguration Load(string filePath) {
        IConfigurationFile file = (IConfigurationFile)Activator.CreateInstance(this.Loader, filePath)!;
        IConfiguration configuration = file.Read() ?? throw new InvalidOperationException("Configuration is null");

        configuration.ConfigurationChanged += (sender, e) => {
            OnConfigurationChanged(filePath);
        };

        this.Configuration = configuration;

        return configuration;
    }

    public void Save(string filePath, IConfiguration configuration) {
        IConfigurationFile file = (IConfigurationFile)Activator.CreateInstance(this.Loader, filePath)!;
        file.Save(configuration);
    }

    private void OnConfigurationChanged(string filePath) {
        this.Save(filePath, this.Configuration!);
    }
}
