using System.Windows;
using EasySave.Logger; // selon ton namespace
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Input;

namespace EasySave.Views
{
    public partial class LogListWindow : Window
    {
        public ObservableCollection<Log> Logs { get; set; }

        public LogListWindow()
        {
            InitializeComponent();
            var logReader = new LogFileJSON(); 
            List<Log> logs = logReader.Read("logs.json");

            Logs = new ObservableCollection<Log>(logs);
            DataContext = this;
        }
        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var grid = sender as DataGrid;
            if (grid.SelectedItem is Log selectedLog)
            {
                var detailWindow = new LogDetailWindow(selectedLog);
                detailWindow.ShowDialog(); // modal
            }
        }
        private void QuitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}