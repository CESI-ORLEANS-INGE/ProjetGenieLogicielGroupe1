using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.Model;

public class BackupRenameTask : BackupTask {
    public BackupRenameTask(IEntryHandler? source, IEntryHandler? destination) : base(source, destination) {
        if (source == null) {
            throw new ArgumentNullException(nameof(source), "Source cannot be null.");
        }
        if (destination == null) {
            throw new ArgumentNullException(nameof(destination), "Destination cannot be null.");
        }
    }

    public override void Run() {
        this.Destination!.Rename(this.Source!.GetName());
    }
}

