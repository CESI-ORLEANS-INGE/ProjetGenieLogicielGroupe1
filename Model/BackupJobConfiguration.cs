using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.Model {
    public interface IBackupJobConfiguration {
        string Name { get; set; }
        string Source { get; set; }
        string Destination { get; set; }
        string Type { get; set; }
        event EventHandler JobConfigurationChanged;
    }

    public class BackupJobConfiguration : IBackupJobConfiguration {
        private string? _Name;
        private string? _Source;
        private string? _Destination;
        private string? _Type;

        public string Name {
            get => _Name ?? throw new InvalidOperationException("Name is not set");
            set {
                this._Name = value;
                this.OnJobConfigurationChanged();
            }
        }

        public string Source {
            get => _Source ?? throw new InvalidOperationException("Source is not set");
            set {
                this._Source = value;
                this.OnJobConfigurationChanged();
            }
        }

        public string Destination {
            get => _Destination ?? throw new InvalidOperationException("Destination is not set");
            set {
                this._Destination = value;
                this.OnJobConfigurationChanged();
            }
        }

        public string Type {
            get => _Type ?? throw new InvalidOperationException("Type is not set");
            set {
                this._Type = value;
                this.OnJobConfigurationChanged();
            }
        }

        public void OnJobConfigurationChanged() {
            this.JobConfigurationChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler? JobConfigurationChanged;
    }
}
