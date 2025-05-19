using System.IO;
using System.Security.Cryptography;
using System.Windows.Controls;
using Microsoft.Win32;

namespace EasySave.Views
{
    partial class Configuration : UserControl
    {
        public Configuration()
        {
            InitializeComponent();
            this.DataContext = new ViewModel();
        }
        private void StateFileClick (object sender, EventArgs e) {
            var dialog = new OpenFileDialog
            {
                Filter = "Fichiers JSON (*.json)|*.json",
                InitialDirectory = Directory.GetCurrentDirectory()
            };

            if (dialog.ShowDialog() == true)
            {
                string selectedFilePath = dialog.FileName;
                if (DataContext is ViewModel vm)
                {
                    vm.LogFile = selectedFilePath;
                }
            }
        }
        private void LogFileClick (object sender, EventArgs e) {
            var dialog = new OpenFileDialog
            {
                Filter = "Fichiers JSON (*.json)|*.json|Fichiers XML (*.xml)|*.xml",
                InitialDirectory = Directory.GetCurrentDirectory()
            };

            if (dialog.ShowDialog() == true)
            {
                string selectedFilePath = dialog.FileName;
                if (DataContext is ViewModel vm)
                {
                    vm.LogFile = selectedFilePath;
                }
            }
        }
    }
}