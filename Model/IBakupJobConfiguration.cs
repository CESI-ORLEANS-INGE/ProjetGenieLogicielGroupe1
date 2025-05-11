using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CONFIGURATION
{
    internal interface IBakupJobConfiguration
    {
        string Name;
        string Source;
        string Destination;
        string Type;
        event EventHandler JobConfigurationChanged;  
    }
}
