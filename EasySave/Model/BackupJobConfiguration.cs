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
            get => _Name ?? string.Empty;
            set {
                this._Name = value;
                this.OnJobConfigurationChanged();
            }
        }

        public string Source {
            get => _Source ?? string.Empty;
            set {
                this._Source = value;
                this.OnJobConfigurationChanged();
            }
        }

        public string Destination {
            get => _Destination ?? string.Empty;
            set {
                this._Destination = value;
                this.OnJobConfigurationChanged();
            }
        }

        public string Type {
            get => _Type ?? "Complete";
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
