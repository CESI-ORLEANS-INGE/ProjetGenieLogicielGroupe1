using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace EasySave.Model;

public class BackupCopyTask : BackupTask {
    public BackupCopyTask(IEntryHandler? source, IEntryHandler? destination) : base(source, destination) {
        if (source == null) {
            throw new ArgumentNullException(nameof(source), "Source cannot be null.");
        }
        if (destination == null) {
            throw new ArgumentNullException(nameof(destination), "Destination cannot be null.");
        }
    }

    protected override void Algorithm() {
        this.Source!.Copy(this.Destination!.GetParent(), true);
        Configuration config = new Configuration();
        Crypto crypto = new Crypto(config);
        string extension = Path.GetExtension(Destination.GetPath()).ToLower();
        if (!config.CryptExt.Select(ext => ext.ToLower()).Contains(extension))
        {
            CryptDuration = crypto.Crypt(Destination.GetPath());
        }
    }
}

  