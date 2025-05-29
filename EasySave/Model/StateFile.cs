using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;


namespace EasySave.Model;

/// <summary>
/// Data Transfer Object (DTO) representing the state of a backup job.
/// Used for serialization/deserialization of backup job states.
/// </summary>
public class JobStateDto {
    public string Name { get; set; }
    public string SourceFilePath { get; set; }
    public string TargetFilePath { get; set; }
    public double TotalFilesToCopy { get; set; }
    public double TotalFilesSize { get; set; }
    public double NbFilesLeftToDo { get; set; }
    public int Progression { get; set; }
    public string State { get; set; }
}

/// <summary>
/// Interface defining how to persist and load backup job states.
/// </summary>
public interface IStateFile {
    /// <summary>
    /// Saves the current list of backup job states to a file.
    /// </summary>
    public void Save(List<IBackupJobState> jobsState);
}

public class StateFile(string filePath) : IStateFile {
    private string _FilePath { get; set; } = filePath;
    private object _LockObject { get; } = new object();
    private JsonSerializerOptions _SerializerOptions { get; } = new() { WriteIndented = true };

    public void Save(List<IBackupJobState> jobsState) {
        var dtoList = new List<JobStateDto>();

        foreach (var jobstate in jobsState) {
            var dto = new JobStateDto {
                Name = jobstate.BackupJob.Name,
                SourceFilePath = jobstate.SourceFilePath,
                TargetFilePath = jobstate.DestinationFilePath,
                TotalFilesToCopy = jobstate.TotalFilesToCopy,
                TotalFilesSize = jobstate.TotalFilesSize,
                NbFilesLeftToDo = jobstate.FilesLeft,
                Progression = jobstate.Progression,
                State = jobstate.State.ToString(),
            };
            dtoList.Add(dto);
        }
        var jsonString = JsonSerializer.Serialize(dtoList, _SerializerOptions);

        lock (this._LockObject) {
            File.WriteAllText(this._FilePath, jsonString);
        }
    }
}

