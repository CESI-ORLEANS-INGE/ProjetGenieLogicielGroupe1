using System;
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
    /// <summary>
    /// Reads and restores the list of backup job states from a file.
    /// </summary>
    public List<IBackupJobState> Read();
}

public class StateFile(string filePath) : IStateFile {
    private string FilePath { get; set; } = filePath;

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
        var jsonString = JsonSerializer.Serialize(dtoList, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(this.FilePath, jsonString);
    }

    public List<IBackupJobState> Read() {
        throw new NotImplementedException("This method is not implemented yet");
        /*
        string jsonString = File.ReadAllText("c:\\temp\\state.json");


        var dtoList = JsonSerializer.Deserialize<List<JobStateDto>>(jsonString);
        var result = new List<IBackupJobState>();

        foreach (var dto in dtoList)
        {
            IBackupJob Job = new BackupJob()
            {
                Name = dto.Name
            };

            BackupJobState jobState = new BackupJobState(Job)
            {
                SourceFilePath = dto.SourceFilePath,
                DestinationFilePath = dto.TargetFilePath,
                TotalFilesToCopy = dto.TotalFilesToCopy,
                TotalFilesSize = dto.TotalFilesSize,
                FilesLeft = dto.NbFilesLeftToDo,
                FilesLeftSize = dto.TotalFilesSize - (dto.TotalFilesSize * dto.Progression / 100.0),
                Progression = dto.Progression,
                State = Enum.Parse<State>(dto.State)
            };

            result.Add((IBackupJobState)jobState);
        }

        return result;
        */
    }
}

