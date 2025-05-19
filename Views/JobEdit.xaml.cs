using System;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Forms; // Ajout de cette directive

namespace EasySave.Views
{
    public partial class JobEdit : Window
    {
        public JobEdit()
        {
            InitializeComponent();
        }
        // Event handler for the Save button click

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrWhiteSpace(PathSource.Text) || string.IsNullOrWhiteSpace(PathDestination.Text))
                {
                    System.Windows.MessageBox.Show("Veuillez remplir tous les champs.");
                    return;
                }
                // Save the job configuration (pseudo code)
                // SaveJobConfiguration(PathSource.Text, PathDestination.Text);
                System.Windows.MessageBox.Show("Sauvegarde réussie !");
                this.Close();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Erreur lors de la sauvegarde : " + ex.Message);
            }
        }
        // Event handler for the Cancel button click
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void DeleteJob(object sender, RoutedEventArgs e)
        {
            try
            {
                // Show confirmation dialog
                ConfirmDeleteWindow confirmDeleteWindow = new ConfirmDeleteWindow();
                if (confirmDeleteWindow.ShowDialog() == true && confirmDeleteWindow.IsConfirmed)
                {
                    // Delete the job (pseudo code)
                    // DeleteJob();
                    System.Windows.MessageBox.Show("Suppression réussie !");
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Erreur lors de la suppression : " + ex.Message);
            }
        }

        // Event handler for the Browse Source button click
        private void BrowseSource_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog
                {
                    Description = "Sélectionnez un dossier source",
                    ShowNewFolderButton = true
                })
                {
                    if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        PathSource.Text = folderBrowserDialog.SelectedPath;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Erreur lors de la sélection du dossier source : " + ex.Message);
            }
        }

        // Event handler for the Browse Destination button click
        private void BrowseDestination_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog
                {
                    Description = "Sélectionnez un dossier de destination",
                    ShowNewFolderButton = true
                })
                {
                    if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        PathDestination.Text = folderBrowserDialog.SelectedPath;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Erreur lors de la sélection du dossier de destination : " + ex.Message);
            }
        }
    }
}