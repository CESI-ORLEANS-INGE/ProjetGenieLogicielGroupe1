using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.Model;

public interface IEntry {
    string GetName();
    string GetPath();
    double GetSize();
    void Remove();
    void Move(IDirectory destination);
    void Copy(IDirectory destination);
    void Rename(string newName);
    bool Exists();
}

internal class EntryHandler : IEntry {
}
