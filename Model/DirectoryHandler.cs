using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace EasySave.Model;

public interface IDirectoryHandler : IEntryHandler {
    /// <summary>
    /// Return the list of entries in the directory
    /// </summary>
    /// <returns>The list of entries in the directory</returns>
    /// <exception cref="DirectoryNotFoundException">If the directory does not exist</exception>
    public List<IEntryHandler> GetEntries();

    /// <summary>
    /// Return the parent directory of the current directory
    /// </summary>
    public IDirectoryHandler GetParent();
}

public class DirectoryHandler(string path) : EntryHandler(path), IDirectoryHandler {

    public override string GetName() {
        return Path.GetFileName(this._Path);
    }

    public override double GetSize() {
        if (!this.Exists()) {
            throw new DirectoryNotFoundException("Directory not found");
        }
        return new DirectoryInfo(this._Path).EnumerateFiles("*", SearchOption.AllDirectories).Sum(file => file.Length);
    }

    public override void Remove() {
        if (!this.Exists()) {
            throw new DirectoryNotFoundException("Directory not found");
        }
        Directory.Delete(this._Path, true);
    }

    public override void Move(IDirectoryHandler destination, bool forceOverride = false) {
        if (this.Exists()) {
            DirectoryHandler destinationDirectory = new(Path.Combine(destination.GetPath(), this.GetName()));
            if (destinationDirectory.Exists() && !forceOverride) {
                throw new IOException("Directory already exists");
            }
            Directory.Move(this._Path, destinationDirectory.GetPath());
            this._Path = destinationDirectory.GetPath();
        } else {
            throw new DirectoryNotFoundException("Directory not found");
        }
    }

    public override void Copy(IDirectoryHandler destination, bool forceOverride = false) {
        if (this.Exists()) {
            DirectoryHandler destinationDirectory = new(Path.Combine(destination.GetPath(), this.GetName()));
            if (destinationDirectory.Exists() && !forceOverride) {
                throw new IOException("Directory already exists");
            }
            Directory.CreateDirectory(destinationDirectory.GetPath());
            foreach (var file in Directory.GetFiles(this._Path)) {
                File.Copy(file, Path.Combine(destinationDirectory.GetPath(), Path.GetFileName(file)), forceOverride);
            }
        } else {
            throw new DirectoryNotFoundException("Directory not found");
        }
    }

    public override void Rename(string newName, bool forceOverride = false) {
        if (this.Exists()) {
            DirectoryHandler destinationDirectory = new(Path.Combine(this.GetParent().GetName(), newName));
            if (destinationDirectory.Exists() && !forceOverride) {
                throw new IOException("Directory already exists");
            }
            Directory.Move(this._Path, destinationDirectory.GetPath());
            this._Path = destinationDirectory.GetPath();
        } else {
            throw new DirectoryNotFoundException("Directory not found");
        }
    }

    public override bool Exists() {
        return Directory.Exists(this._Path);
    }

    public List<IEntryHandler> GetEntries() {
        if (!this.Exists()) {
            throw new DirectoryNotFoundException("Directory not found");
        }
        return [.. Directory.GetFileSystemEntries(this._Path).Select(entry => {
            if (Directory.Exists(entry)) {
                return new DirectoryHandler(entry) as IEntryHandler;
            } else {
                return new FileHandler(entry) as IEntryHandler;
            }
        })];
    }

    public IDirectoryHandler GetParent() {
        if (!this.Exists()) {
            throw new DirectoryNotFoundException("Directory not found");
        }

        string parentName = (Directory.GetParent(this._Path)?.FullName) ?? throw new DirectoryNotFoundException("Parent directory not found");

        return new DirectoryHandler(parentName);
    }
}

