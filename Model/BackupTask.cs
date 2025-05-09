using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.Model;

public interface IBackupTask {
    public IEntry Source { get; }
    public IEntry Destination { get; }
    public DateTime StartTime { get; }
    public DateTime EndTime { get; }

    public double GetDuration();
    public void Run();
} 

internal class BackupTask {
} 

