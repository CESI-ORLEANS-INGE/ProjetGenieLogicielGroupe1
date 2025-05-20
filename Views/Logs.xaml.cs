using System.Windows;
using EasySave.Logger; // selon ton namespace
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;
using System.Linq;

namespace EasySave.Views;
public partial class Logs : INotifyPropertyChanged {
    public IViewModel ViewModel { get; private set; }
    public ObservableCollection<Log> LogCollection { get; set; }
    public ObservableCollection<Log> PagedLogCollection { get; set; } = new();
    public int PageSize { get; set; } = 20;
    private int _currentPage = 1;
    public int CurrentPage {
        get => _currentPage;
        set {
            if (_currentPage != value) {
                _currentPage = value;
                OnPropertyChanged(nameof(CurrentPage));
                UpdatePagedLogCollection();
                OnPropertyChanged(nameof(PageIndicator));
                OnPropertyChanged(nameof(IsPreviousEnabled));
                OnPropertyChanged(nameof(IsNextEnabled));
            }
        }
    }
    public int TotalPages => (LogCollection.Count + PageSize - 1) / PageSize;
    public string PageIndicator => $"Page {CurrentPage} of {TotalPages}";
    public bool IsPreviousEnabled => CurrentPage > 1;
    public bool IsNextEnabled => CurrentPage < TotalPages;

    public event PropertyChangedEventHandler? PropertyChanged;

    public Logs(IViewModel viewModel) {
        this.ViewModel = viewModel;

        InitializeComponent();

        var logReader = new LogFileJSON();
        List<Log> logs = logReader.Read(viewModel.Configuration.LogFile);

        LogCollection = [.. logs];
        UpdatePagedLogCollection();
        MainGrid.DataContext = this;

        //Task.Run(() => {
        //    Dispatcher.Invoke(() => {
        //        while (true) {
        //            List<Log> logs = logReader.Read(viewModel.Configuration.LogFile);

        //            if (logs.Count > LogCollection.Count) {
        //                foreach (var log in logs.Skip(LogCollection.Count))
        //                    LogCollection.Add(log);
        //                UpdatePagedLogCollection();
        //            }

        //            Task.Delay(1000).Wait();
        //        }
        //    });
        //});
    }

    private void UpdatePagedLogCollection() {
        PagedLogCollection.Clear();
        var items = LogCollection.Skip((CurrentPage - 1) * PageSize).Take(PageSize);
        foreach (var log in items)
            PagedLogCollection.Add(log);
        OnPropertyChanged(nameof(PagedLogCollection));
        OnPropertyChanged(nameof(PageIndicator));
        OnPropertyChanged(nameof(IsPreviousEnabled));
        OnPropertyChanged(nameof(IsNextEnabled));
    }

    private void PreviousPage_Click(object sender, RoutedEventArgs e) {
        if (CurrentPage > 1)
            CurrentPage--;
    }

    private void NextPage_Click(object sender, RoutedEventArgs e) {
        if (CurrentPage < TotalPages)
            CurrentPage++;
    }

    private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
        var grid = sender as DataGrid;
        if (grid?.SelectedItem is Log selectedLog) {
            var detailWindow = new LogDetailsWindow(selectedLog);
            detailWindow.ShowDialog(); // modal
        }
    }

    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
