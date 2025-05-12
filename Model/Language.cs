using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EasySave.Model;

public interface ILanguage {
    // Interface for Language class
    /// <summary>
    /// Dictionary containing the translations for the language
    /// </summary>
    public Dictionary<string, string> Traductions { get; set; }

    /// <summary>
    /// Current language
    /// </summary>
    /// <param name="language"></param>
    void SetLanguage(string language);

    /// <summary>
    /// Get the current language
    /// </summary>
    /// <returns></returns>
    string GetLanguage();
    /// <summary>
    /// Load the language from the file
    /// </summary>
    void Load();
    event EventHandler LanguageChanged;
}

public class Language : ILanguage {
    // Singleton instance of Language
    public static Language Instance { get; } = new Language();
    // Private constructor to prevent instantiation from outside
    private Language() { }
    public Dictionary<string, string> Traductions { get; set; } = [];


    // Initialize the event with an empty delegate to avoid null issues  
    public event EventHandler LanguageChanged = delegate { };

    public void SetLanguage(string _language) {
        Configuration.Instance!.Language = _language;
        OnLanguageChanged();
    }

    public string GetLanguage() {
        return Configuration.Instance!.Language;
    }

    public void Load() {
        // Implementation for loading language data
        // Load the language from the file
        //Extract the language from the json file in the path "Resources/Language/{language}.json"

        // Deserialize the json file into a Dictionary<string, string>
        // and assign it to the Traductions property

        string json = File.ReadAllText($"Resources/Language/{Configuration.Instance!.Language}.json");
        this.Traductions = JsonSerializer.Deserialize<Dictionary<string, string>>(json)!;
    }

    protected virtual void OnLanguageChanged() {
        LanguageChanged?.Invoke(this, EventArgs.Empty);
    }
}
