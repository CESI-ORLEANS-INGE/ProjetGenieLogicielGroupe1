using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyRemote
{
    public interface IBackupJob
    {
        public string Name { get; set; }
        IDirectory Source { get; set; }
        IDirectory Destination { get; set; }
    }
    class BackupJob
    {
        public string Name { get; set; }
        IDirectory Source { get; set; }
        IDirectory Destination { get; set; }
    }
}
