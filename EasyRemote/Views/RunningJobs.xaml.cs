using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EasyRemote.Views
{
    partial class RunningJobs : INotifyPropertyChanged
    {
        public IViewModel ViewModel { get; private set; }

        public RunningJobs(IViewModel viewModel)
        {
            ViewModel = viewModel;

            InitializeComponent();

            
}


