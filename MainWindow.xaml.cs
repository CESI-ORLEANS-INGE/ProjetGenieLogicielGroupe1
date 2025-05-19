using System.Windows;
using EasySave.Views;

namespace EasySave;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window {
    public IViewModel ViewModel { get; private set; }

    public MainWindow(IViewModel viewModel) {
        this.ViewModel = viewModel;

        InitializeComponent();

        this.MainGrid.DataContext = this;
    }

    private void JobsList_Click(object sender, RoutedEventArgs e) {
        MainContent.Content = new JobsList();
    }
    private void RunningJobs_Click(object sender, RoutedEventArgs e) {
        MainContent.Content = new RunningJobs(this.ViewModel);
    }
    private void Logs_Click(object sender, RoutedEventArgs e) {
        MainContent.Content = new Logs();
    }
    private void Configuration_Click(object sender, RoutedEventArgs e) {
        MainContent.Content = new Configuration();
    }

}