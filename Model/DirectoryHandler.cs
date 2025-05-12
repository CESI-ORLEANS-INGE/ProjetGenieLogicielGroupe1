using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.Model;

public interface IDirectory : IEntry {
    List<IEntry> GetEntries();
}

internal class DirectoryHandler : IDirectory {
}

