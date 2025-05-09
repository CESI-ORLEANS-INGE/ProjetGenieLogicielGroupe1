using System;
using System.Collections.Generic;
using IViewModel;

namespace EasySave.Model

public class IConfigurationManager 
{
    IFile File;
    IConfiguration Load(string path);
    void OnConfigurationChanged();
}