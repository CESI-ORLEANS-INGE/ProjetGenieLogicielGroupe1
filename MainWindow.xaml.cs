using System.Windows;
using EasySave.Views;

namespace EasySave;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }
    
    private void JobsList_Click(object sender, RoutedEventArgs e)
    {
        MainContent.Content = new JobsList();
    }

    private void RunningJobs_Click(object sender, RoutedEventArgs e)
    {
        MainContent.Content = new RunningJobs();
    }
    private void Logs_Click(object sender, RoutedEventArgs e)
    {
        MainContent.Content = new Logs();
        var Liste= new LogListWindow();
    }
    private void Configuration_Click(object sender, RoutedEventArgs e)
    {
        MainContent.Content = new Configuration();
    }

}