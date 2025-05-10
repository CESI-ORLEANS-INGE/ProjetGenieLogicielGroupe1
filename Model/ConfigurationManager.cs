using System;
using System.Collections.Generic;
using IViewModel;

namespace EasySave.Model;

public interface IConfigurationManager {
    IFile File;
    Configuration Load(string path);
    void OnConfigurationChanged();
}

public class ConfigurationManager : IConfigurationManager {
}