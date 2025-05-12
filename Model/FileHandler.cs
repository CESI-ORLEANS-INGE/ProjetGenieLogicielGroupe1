 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.Model;

public interface IFile : IEntry {
    string GetExtension();
    void Write(string content);
    void Append(string content);
    IConfiguration Load(string path);
}

internal class FileHandler : IFile {
}

