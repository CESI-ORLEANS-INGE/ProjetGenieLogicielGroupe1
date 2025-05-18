using System;
using Microsoft.Win32;
using System.Windows;

namespace EasySave.Views
{
    public partial class JobEdit : Window
    {
        // Constructor for the JobEdit window
        public JobEdit()
        {
            InitializeComponent();
        }

        // Event handler for the Save button click
        private void SaveJob(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Event handler for the Cancel button click
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Event handler for the Delete button click
        private void DeleteJob(object sender, RoutedEventArgs e)
        {
            // Open confirmation dialog
            ConfirmDeleteWindow confirmDeleteWindow = new ConfirmDeleteWindow();
            confirmDeleteWindow.ShowDialog();
        }

        // Event handler for the Browse Source button click
        private void BrowseSource_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Title = "Sélectionnez un fichier source",
                    Filter = "Tous les fichiers (*.*)|*.*"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    PathSource.Text = openFileDialog.FileName;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de la sélection du fichier source : " + ex.Message);
            }
        }

        // Event handler for the Browse Destination button click
        private void BrowseDestination_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Title = "Sélectionnez un fichier de destination",
                    Filter = "Tous les fichiers (*.*)|*.*"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    PathDestination.Text = openFileDialog.FileName;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de la sélection du fichier de destination : " + ex.Message);
            }
        }
    }
}
