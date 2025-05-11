using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.Model;

public class BackupRemoveTask : BackupTask {
    public BackupRemoveTask(IEntryHandler? source, IEntryHandler? destination) : base(source, destination) {
        if (destination == null) {
            throw new ArgumentNullException(nameof(destination), "Destination cannot be null.");
        }
    }

    public override void Run() {
        this.Destination!.Remove();
    }
}
