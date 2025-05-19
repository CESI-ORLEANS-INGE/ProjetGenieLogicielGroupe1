using EasySave.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Controls;
using System.ComponentModel;

namespace EasySave.Views {
    partial class RunningJobs : INotifyPropertyChanged {
        private readonly IViewModel _ViewModel;
        private readonly ObservableCollection<IBackupJobState> _RunningJobList = [];
        public IEnumerable<IBackupJobState> RunningJobList => _RunningJobList;
        private readonly Task _RefreshTask;
        public DateTime? StartedAt => _RunningJobList.Count == 0 ? null : _RunningJobList.Min(job => job.BackupJob.StartedAt);
        public int TotalFilesToCopy => _RunningJobList.Count == 0 ? 0 : (int)_RunningJobList.Sum(job => job.TotalFilesToCopy);
        public int TotalFilesLeft => _RunningJobList.Count == 0 ? 0 : (int)_RunningJobList.Sum(job => job.FilesLeft);

        public RunningJobs(IViewModel viewModel) {
            this._ViewModel = viewModel;

            InitializeComponent();

            this.MainGrid.DataContext = this;

            this._ViewModel.JobStateChanged += this.OnJobStateChanged;

            this._RefreshTask = Task.Run(() => {
                while (true) {
                    this.Dispatcher.Invoke(() => {
                        this.RunningJobsList.Items.Refresh();
                        this.OnPropertyChanged(nameof(StartedAt));
                    });
                    Task.Delay(1000).Wait();
                }
            });
        }

        private void OnJobStateChanged(object sender, JobStateChangedEventArgs e) {
            this.Dispatcher.Invoke(() => {
                if (e.JobState is not null) {
                    this._RunningJobList.Clear();
                    foreach (IBackupJobState jobState in this._ViewModel.BackupState.JobState) {
                        if (jobState.State == State.ACTIVE || jobState.State == State.IN_PROGRESS || jobState.State == State.PAUSED) {
                            this._RunningJobList.Add(jobState);
                        }
                    }
                }
                this.RunningJobsList.Items.Refresh();
            });

            this.OnPropertyChanged(nameof(StartedAt));
            this.OnPropertyChanged(nameof(TotalFilesToCopy));
            this.OnPropertyChanged(nameof(TotalFilesLeft));
        }

        private void CancelAllButton_Click(object sender, RoutedEventArgs e) {
            foreach (IBackupJob job in _ViewModel.BackupJobs) {
                job.Stop();
            }
        }

        private void RunAllButton_Click(object sender, RoutedEventArgs e) {
            this._ViewModel.RunCommandRun([.. this._ViewModel.Configuration.Jobs.Select(j => j.Name)]);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            if (sender is Button button && button.DataContext is IBackupJobState jobState) {
                jobState.BackupJob.Stop();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
