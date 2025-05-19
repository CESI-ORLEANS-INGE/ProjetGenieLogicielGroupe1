using System.Windows;
using EasySave.Views;

namespace EasySave;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window {
    private readonly IViewModel _ViewModel;

    public MainWindow(IViewModel viewModel) {
        this._ViewModel = viewModel;

        InitializeComponent();
    }

    private void JobsList_Click(object sender, RoutedEventArgs e) {
        MainContent.Content = new JobsList();
    }
    private void RunningJobs_Click(object sender, RoutedEventArgs e) {
        MainContent.Content = new RunningJobs(this._ViewModel);
    }
    private void Logs_Click(object sender, RoutedEventArgs e) {
        MainContent.Content = new Logs();
    }
    private void Configuration_Click(object sender, RoutedEventArgs e) {
        MainContent.Content = new Configuration(this._ViewModel);
    }

}