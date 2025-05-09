using System;
using System.Collections.Generic;
using IViewModel;

namespace CONFIGURATION

public class IConfigurationManager 
{
    IFile File;
    IConfiguration Load(string path);
    void OnConfigurationChanged();
}