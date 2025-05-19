using System.Windows;
using EasySave.Logger;

namespace EasySave.Views;

public partial class LogDetailsWindow : Window {
    public LogDetailsWindow(Log selectedLog) {
        InitializeComponent();
        DataContext = selectedLog;
    }

    private void Ok_Click(object sender, RoutedEventArgs e) {
        this.Close();
    }
}

