using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using IConfigurationManager;

public interface IConfigurationFile
{
    void Save(JsonArray configuration) ;
    JsonArray ReaderWriterLock(string path);
}