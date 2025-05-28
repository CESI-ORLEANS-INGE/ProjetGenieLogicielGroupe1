using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EasyRemote.Views
{
    partial class RunningJobs : INotifyPropertyChanged
    {
        public IViewModel ViewModel { get; private set; }
        private readonly ObservableCollection<IBackupJobState> _RunningJobList = [];
        public IEnumerable<IBackupJobState> RunningJobList => _RunningJobList;
        private readonly Task _RefreshTask;
        public DateTime? StartedAt => _RunningJobList.Count == 0 ? null : _RunningJobList.Min(job => job.BackupJob.StartedAt);
        public int TotalFilesToCopy => _RunningJobList.Count == 0 ? 0 : (int)_RunningJobList.Sum(job => job.TotalFilesToCopy);
        public int TotalFilesLeft => _RunningJobList.Count == 0 ? 0 : (int)_RunningJobList.Sum(job => job.FilesLeft);
        public int Progression => _RunningJobList.Count == 0 ? 0 : _RunningJobList.Sum(job => job.Progression) / _RunningJobList.Count;

        public RunningJobs(IViewModel viewModel)
        {
            ViewModel = viewModel;

            InitializeComponent();

            this.MainGrid.DataContext = this;

            ViewModel.JobStateChanged += this.OnJobStateChanged;

            _RefreshTask = Task.Run(() =>
            {
                while (true)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        this.RunningJobsList.Items.Refresh();
                        OnPropertyChanged(nameof(StartedAt));
                    });
                    Task.Delay(1000).Wait();
                }
            });

            this.Dispatcher.Invoke(() =>
            {
                UpdateList();
            });

            UpdateStats();
        }

        private void UpdateList()
        {
            _RunningJobList.Clear();

            if (ViewModel.BackupState is null)
            {
                return;
            }

            foreach (IBackupJobState jobState in ViewModel.BackupState.JobState)
            {
                if (jobState.State == State.ACTIVE || jobState.State == State.IN_PROGRESS || jobState.State == State.PAUSED)
                {
                    _RunningJobList.Add(jobState);
                }
            }
            this.RunningJobsList.Items.Refresh();
        }

        private void UpdateStats()
        {
            OnPropertyChanged(nameof(StartedAt));
            OnPropertyChanged(nameof(TotalFilesToCopy));
            OnPropertyChanged(nameof(TotalFilesLeft));
            OnPropertyChanged(nameof(Progression));
        }

        private void OnJobStateChanged(object sender, JobStateChangedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                if (e.JobState is not null)
                {
                    UpdateList();
                }
            });

            UpdateStats();
        }

        private void CancelAllButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (IBackupJob job in ViewModel.BackupJobs)
            {
                job.Stop();
            }
        }

        private void RunAllButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.RunCommandRun([.. ViewModel.Configuration.Jobs.Select(j => j.Name)]);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button && button.DataContext is IBackupJobState jobState)
            {
                jobState.BackupJob.Stop();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

}
