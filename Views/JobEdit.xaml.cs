using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Windows;

using System.Windows.Controls;
using EasySave.Views;

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
        private void Cancel(object sender, RoutedEventArgs e)
        {
            // Logic to cancel the job creation
            // For example, you can just close the window
            this.Close();
        }

        // Event handler for the Delete button click
        private void DeleteJob(object sender, RoutedEventArgs e)
        {
            // Logic to delete the job
            // For example, you can remove the job from a list or database
            // Close the window after deleting
            this.Close();
        }

        // Generation of file system windows
        // Event handler for the Browse button click
        private void BrowseSource_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                PathSource.Text = openFileDialog.FileName;
            }
        }

        private void BrowseDestination_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                PathDesstination.Text = openFileDialog.FileName;
            }
        }

    }
}
