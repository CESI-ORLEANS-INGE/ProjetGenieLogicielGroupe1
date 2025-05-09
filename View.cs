using System;
using EasySave.Model;
using Logger;

namespace EasySave;

public interface IView {
    IViewModel ViewModel { get; set; }

    // +----------------------------------+
    // |          EVENTS HANDLERS         |
    // +----------------------------------+
    void OnPropertyChanged(string propertyName);
    void OnLanguageChanged(string language);
    void OnJobStateChanged(IBackupJobState jobState);
    void OnConfigurationChanged();
}

public class View : IView {
    static public void Main(string[] args) {
        Console.WriteLine("Hello World!");
    }
}