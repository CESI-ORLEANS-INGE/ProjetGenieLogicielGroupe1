using System;
using System.Collections.Generic;

namespace EasySave.Model;

public interface IConfigurationManager {
    IFileHandler File;
    Configuration Load(string path);
    void OnConfigurationChanged();
}

public class ConfigurationManager : IConfigurationManager {
}