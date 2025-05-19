using System.Windows.Controls;
using EasySave.Views;

namespace EasySave.Views
{
    public partial class JobsList : System.Windows.Controls.UserControl
    {
        public JobsList()
        {
            InitializeComponent();
        }
        private void AddJob(object sender, System.Windows.RoutedEventArgs e)
        {
            JobEdit jobEdit = new JobEdit();
            jobEdit.Show();
        }
    }
}
