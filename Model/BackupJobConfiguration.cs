﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.Model {
    public interface IBackupJobConfiguration {
        string Name;
        string Source;
        string Destination;
        string Type;
        event EventHandler JobConfigurationChanged;
    }

    public class BackupJobConfiguration : IBackupJobConfiguration {
    }
}
