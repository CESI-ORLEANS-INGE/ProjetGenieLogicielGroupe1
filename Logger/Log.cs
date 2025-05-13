using System;

namespace EasySave.Model
{
    // Enumeration of different logging levels
    public enum LogLevel
    {
        Debug,         // Detailed technical information for debugging
        Information,   // General information about normal operations
        Warning,       // Warnings about non-critical issues
        Error,         // Errors that may interrupt a process
        Critical       // Serious errors that may cause application failure
    }

    // Interface representing the structure of a log entry
    public interface ILog
    {
        public DateTime Datetime { get; set; }             // Date and time when the log was recorded
        public string Name { get; set; }   // Name 
        public string Description { get; set; }     // Description of the log content
        public string Type { get; set; }            // Type of the operation 
        public string TypeDescription { get; set; }  // Text description of the operation type
        public double Filesize { get; set; }               // Size of the file being processed
        public double TransfertDuration { get; set; }      // Duration of the transfer in seconds
        public LogLevel Level { get; set; }                // Severity level of the log
    }

    // Concrete class implementing the ILog interface
    public class Log : ILog
    {
        public DateTime Datetime { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string TypeDescription { get; set; } = string.Empty;
        public double Filesize { get; set; }
        public double TransfertDuration { get; set; }
        public LogLevel Level { get; set; }
    }
}
