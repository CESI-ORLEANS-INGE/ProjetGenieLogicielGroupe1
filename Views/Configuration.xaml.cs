using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Controls;
using EasySave.Model;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace EasySave.Views {
    partial class Configuration : System.Windows.Controls.UserControl {
        private readonly IViewModel _ViewModel;
        public Configuration(IViewModel viewModel) {
            InitializeComponent();
            this._ViewModel = viewModel;
            this.MainGrid.DataContext = _ViewModel;
        }

        private void StateFileClick(object sender, EventArgs e) {
            var dialog = new Microsoft.Win32.OpenFileDialog {
                Filter = "Fichiers JSON (*.json)|*.json",
                InitialDirectory = Directory.GetCurrentDirectory()
            };

            if (dialog.ShowDialog() == true) {
                string selectedFilePath = dialog.FileName;
                this._ViewModel.StateFile = selectedFilePath;
            }
        }

        private void LogFileClick(object sender, EventArgs e) {
            var dialog = new Microsoft.Win32.OpenFileDialog {
                Filter = "Fichiers JSON (*.json)|*.json|Fichiers XML (*.xml)|*.xml",
                InitialDirectory = Directory.GetCurrentDirectory()
            };

            if (dialog.ShowDialog() == true) {
                string selectedFilePath = dialog.FileName;
                this._ViewModel.LogFile = selectedFilePath;

            }
        }

        private void CryptoFileClick(object sender, EventArgs e) {
            var dialog = new Microsoft.Win32.OpenFileDialog {
                Filter = "Fichiers exe (*.exe)|*.exe",
                InitialDirectory = Directory.GetCurrentDirectory()
            };

            if (dialog.ShowDialog() == true) {
                string selectedFilePath = dialog.FileName;
                this._ViewModel.CryptoFile = selectedFilePath;

            }
        }

        private void AddExtension_Click(object sender, RoutedEventArgs e) {
            var input = ExtensionInput.Text?.Trim();
            if (string.IsNullOrEmpty(input))
                return;

            if (_ViewModel?.Configuration?.CryptoExtentions != null && !_ViewModel.Configuration.CryptoExtentions.Contains(input)) {
                _ViewModel.Configuration.CryptoExtentions.Add(input);
                ExtensionInput.Clear();
            }
        }

        private void RemoveExtensionItem_Click(object sender, RoutedEventArgs e) {
            if (sender is System.Windows.Controls.Button btn && btn.Tag is string item) {
                _ViewModel?.Configuration?.CryptoExtentions?.Remove(item);
            }
        }

        private void AddProcess_Click(object sender, RoutedEventArgs e) {
            var input = ProcessInput.Text?.Trim();
            if (string.IsNullOrEmpty(input))
                return;

            if (_ViewModel?.Configuration?.Processes != null && !_ViewModel.Configuration.Processes.Contains(input)) {
                _ViewModel.Configuration.Processes.Add(input);
                ProcessInput.Clear();
            }
        }

        private void RemoveProcessItem_Click(object sender, RoutedEventArgs e) {
            if (sender is System.Windows.Controls.Button btn && btn.Tag is string item) {
                _ViewModel?.Configuration?.Processes?.Remove(item);
            }
        }

        private void ClearExtensions_Click(object sender, RoutedEventArgs e) {
            _ViewModel?.Configuration?.CryptoExtentions?.Clear();
        }

        private void ClearProcesses_Click(object sender, RoutedEventArgs e) {
            _ViewModel?.Configuration?.Processes?.Clear();
        }

        private void SelectProcess_Click(object sender, RoutedEventArgs e) {
            var dlg = new SelectProcess {
                Owner = Window.GetWindow(this)
            };
            if (dlg.ShowDialog() == true && !string.IsNullOrWhiteSpace(dlg.SelectedProcessName)) {
                if (_ViewModel?.Configuration?.Processes != null && !_ViewModel.Configuration.Processes.Contains(dlg.SelectedProcessName)) {
                    _ViewModel.Configuration.Processes.Add(dlg.SelectedProcessName);
                }
            }
        }

        private void GenerateEncryptionKey_Click(object sender, RoutedEventArgs e) {
            // Generate a 64-character (256-bit) random hex key
            var bytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create()) {
                rng.GetBytes(bytes);
            }
            var hex = BitConverter.ToString(bytes).Replace("-", "");
            // Set the key in the ViewModel/Configuration
            _ViewModel.Configuration.CryptoKey = hex;
            _ViewModel.OnPropertyChanged("EncryptionKey");
        }
    }
}