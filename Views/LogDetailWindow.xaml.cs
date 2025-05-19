using System.Windows;
using EasySave.Logger;
namespace EasySave.Views
{
    public partial class LogDetailWindow : Window
    {
        public LogDetailWindow(Log selectedLog)
        {
            InitializeComponent();
            DataContext = selectedLog;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }

}
