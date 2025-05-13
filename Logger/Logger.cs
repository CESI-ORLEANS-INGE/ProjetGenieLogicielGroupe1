using System;
using System.Runtime.CompilerServices;

namespace EasySave.Model
{
    // Interface that defines logging methods for different severity levels
    public interface ILogger
    {
        // General logging method
        public void Log(Log entry);

        // Specific methods for each log level
        public void Info(Log entry);
        public void Debug(Log entry);
        public void Warning(Log entry);
        public void Error(Log entry);
        public void Critical(Log entry);
    }

    // Class implementing the logging functionality
    public class Logger : ILogger
    {
        // File path where the logs will be stored
        private readonly string _filePath;

        // Lock object to prevent concurrent file access
        private static readonly object _fileLock = new object();

        // Constructor that allows specifying a custom log file path
        // Defaults to "logs.txt" if no path is provided
        public Logger(string filePath = "logs.txt")
        {
            _filePath = filePath;
        }

        // Core method to log an entry
        public void Log(Log entry)
        {
            // Format the log message with all relevant information
            string logMessage = $"[{entry.Datetime:yyyy-MM-dd HH:mm:ss}] " +
                                $"[{entry.Level}] " +
                                $"Name: {entry.Name}, " +
                                $"Type: {entry.Type} ({entry.TypeDescription}), " +
                                $"Size: {entry.Filesize:0.00} units, " +
                                $"Duration: {entry.TransfertDuration:0.00}s, " +
                                $"Details: {entry.Description}" +
                                $"LogLevel: {entry.Level}";

            // Output the log message to the console
            Console.WriteLine(logMessage);

            // Save the log to the file using the LogFile class
            LogFile File = new();
            File.Save(entry, this._filePath);
        }

        // Shortcut method to log an entry as "Information"
        public void Info(Log entry) => LogWithLevel(entry, LogLevel.Information);

        // Shortcut method to log an entry as "Debug"
        public void Debug(Log entry) => LogWithLevel(entry, LogLevel.Debug);

        // Shortcut method to log an entry as "Warning"
        public void Warning(Log entry) => LogWithLevel(entry, LogLevel.Warning);

        // Shortcut method to log an entry as "Error"
        public void Error(Log entry) => LogWithLevel(entry, LogLevel.Error);

        // Shortcut method to log an entry as "Critical"
        public void Critical(Log entry) => LogWithLevel(entry, LogLevel.Critical);

        // Private helper that sets the log level and timestamp, then calls Log()
        private void LogWithLevel(Log entry, LogLevel level)
        {
            entry.Level = level;              // Set the appropriate log level
            entry.Datetime = DateTime.Now;    // Update the timestamp to the current time
            Log(entry);                       // Call the main Log() method
        }
    }
}
