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
        public IViewModel ViewModel { get; private set; }
        private readonly ObservableCollection<IBackupJobState> _RunningJobList = [];
        public IEnumerable<IBackupJobState> RunningJobList => _RunningJobList;
        private readonly Task _RefreshTask;
        public DateTime? StartedAt => _RunningJobList.Count == 0 ? null : _RunningJobList.Min(job => job.BackupJob.StartedAt);
        public int TotalFilesToCopy => _RunningJobList.Count == 0 ? 0 : (int)_RunningJobList.Sum(job => job.TotalFilesToCopy);
        public int TotalFilesLeft => _RunningJobList.Count == 0 ? 0 : (int)_RunningJobList.Sum(job => job.FilesLeft);
        public int Progression => _RunningJobList.Count == 0 ? 0 : (int)_RunningJobList.Sum(job => job.Progression) / _RunningJobList.Count;

        public RunningJobs(IViewModel viewModel) {
            this.ViewModel = viewModel;

            InitializeComponent();

            this.MainGrid.DataContext = this;

            this.ViewModel.JobStateChanged += this.OnJobStateChanged;

            this._RefreshTask = Task.Run(() => {
                while (true) {
                    this.Dispatcher.Invoke(() => {
                        this.RunningJobsList.Items.Refresh();
                        this.OnPropertyChanged(nameof(StartedAt));
                    });
                    Task.Delay(1000).Wait();
                }
            });

            this.Dispatcher.Invoke(() => {
                this.UpdateList();
            });

            this.UpdateStats();
        }

        private void UpdateList() {
            this._RunningJobList.Clear();

            if (this.ViewModel.BackupState is null) {
                return;
            }

            foreach (IBackupJobState jobState in this.ViewModel.BackupState.JobState) {
                if (jobState.State == State.ACTIVE || jobState.State == State.IN_PROGRESS || jobState.State == State.PAUSED) {
                    this._RunningJobList.Add(jobState);
                }
            }
            this.RunningJobsList.Items.Refresh();
        }

        private void UpdateStats() {
            this.OnPropertyChanged(nameof(StartedAt));
            this.OnPropertyChanged(nameof(TotalFilesToCopy));
            this.OnPropertyChanged(nameof(TotalFilesLeft));
            this.OnPropertyChanged(nameof(Progression));
        }

        private void OnJobStateChanged(object sender, JobStateChangedEventArgs e) {
            this.Dispatcher.Invoke(() => {
                if (e.JobState is not null) {
                    this.UpdateList();
                }
            });

            this.UpdateStats();
        }

        private void CancelAllButton_Click(object sender, RoutedEventArgs e) {
            foreach (IBackupJob job in ViewModel.BackupJobs) {
                job.Stop();
            }
        }

        private void RunAllButton_Click(object sender, RoutedEventArgs e) {
            this.ViewModel.RunCommandRun([.. this.ViewModel.Configuration.Jobs.Select(j => j.Name)]);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            if (sender is System.Windows.Controls.Button button && button.DataContext is IBackupJobState jobState) {
                jobState.BackupJob.Stop();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void PauseOrResumeButton_Click(object sender, RoutedEventArgs e) {
            if (sender is System.Windows.Controls.Button button && button.DataContext is IBackupJobState jobState) {
                if (jobState.BackupJob.IsPaused) {
                    jobState.BackupJob.Resume();
                } else {
                    jobState.BackupJob.Pause();
                }
            }
        }
    }
}
