using System;
using System.Collections.Generic;

using IConfigurationManager;

namespace CONFIGURATION

{
    public interface IConfiguration
    {
        string Language;
        List<IBackupJobConfiguration> Jobs;
        void OnJobConfigurationChanged();
        event EventHandler ConfigurationChanged;
    }
}