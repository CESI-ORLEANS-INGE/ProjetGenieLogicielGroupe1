using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using IConfigurationManager;

namespace EasySave.Model { 
    public interface IConfigurationFile
    {
        void Save(JsonArray configuration) ;
        JsonArray ReaderWriterLock(string path);
    }
}