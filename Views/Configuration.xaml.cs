using System.IO;
using System.Security.Cryptography;
using System.Windows.Controls;
using EasySave.Model;
using Microsoft.Win32;

namespace EasySave.Views
{
    partial class Configuration : System.Windows.Controls.UserControl
    {
        private readonly IViewModel _ViewModel;
        public Configuration(IViewModel viewModel)
        {
            InitializeComponent();
            this._ViewModel = viewModel; 
            this.DataContext = _ViewModel;
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
                    vm.StateFile = selectedFilePath;
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
        private void CryptoFileClick(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Fichiers exe (*.exe)|*.exe",
                InitialDirectory = Directory.GetCurrentDirectory()
            };

            if (dialog.ShowDialog() == true)
            {
                string selectedFilePath = dialog.FileName;
                if (DataContext is ViewModel vm)
                {
                    vm.CryptoFile = selectedFilePath;
                }
            }
        }
        }
}