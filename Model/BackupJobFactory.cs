using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.Model;

public interface IBackupJobFactory {
    IBackupJob Create(IBackupJobConfiguration configuration);
    List<IBackupJob> Create(List<IBackupJobConfiguration> configurations);
} 

internal class BackupJobFactory : IBackupJobFactory {

}

 