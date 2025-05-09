using System;
using System.Collections.Generic;

using IConfigurationManager;

namespace EasySave.Model {
    public interface IConfiguration {
        string Language;
        List<IBackupJobConfiguration> Jobs;
        void OnJobConfigurationChanged();
        event EventHandler ConfigurationChanged;
    }

    public class Configuration : IConfiguration {
    }
}