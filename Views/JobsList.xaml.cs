using System.Windows.Controls;
using EasySave.Views;   
using EasySave.Model;
using System.Windows;


namespace EasySave.Views
{
    public partial class JobsList : System.Windows.Controls.UserControl
    {
        // Déclaration d'une variable pour stocker l'instance de ViewModel
        private object _ViewModel;
        private IViewModel _viewModel ;

        // Constructeur de la classe JobsList
        public JobsList(IViewModel viewModel)
        {
            InitializeComponent(); // Déplacer l'initialisation au début pour accéder aux contrôles  

            _viewModel = viewModel; // Correction de la casse pour correspondre au type défini  

            // Créer une liste observable pour lier au DataGrid  
            var jobsCollection = new System.Collections.ObjectModel.ObservableCollection<Model.IBackupJobConfiguration>();

            // Vérification que l'instance de Configuration n'est pas null avant d'accéder à Jobs  
            if (Model.Configuration.Instance != null)
            {
                // Chargement de la liste des jobs depuis le modèle  
                var jobs = Model.Configuration.Instance.Jobs;
                foreach (var job in jobs)
                {
                    // Ajouter chaque job à la collection observable  
                    jobsCollection.Add(job);
                }

                // Définir la collection comme source de données du DataGrid  
                // Supposons que votre DataGrid s'appelle jobsDataGrid  
                jobsDataGrid.ItemsSource = jobsCollection;
            }
            else
            {
                // Affichage d'un message d'erreur si Configuration est null  
                System.Windows.MessageBox.Show("Erreur de chargement de la configuration.");
            }
        }

        private void AddJob(object sender, System.Windows.RoutedEventArgs e)
        {
            JobEdit jobEdit = new JobEdit(_viewModel); // Correction de la syntaxe pour passer le DataContext  
            jobEdit.Show();
        }

        private void EditJob_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Récupérer le job sélectionné dans le DataGrid  
            var selectedJob = jobsDataGrid.SelectedItem as Model.IBackupJobConfiguration;
            if (selectedJob != null)
            {
                JobEdit jobEdit = new JobEdit(_viewModel);
                jobEdit.Show();
            }
            else
            {
                System.Windows.MessageBox.Show("Veuillez sélectionner un job à modifier.");
            }
        }

        private void DeleteJob_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Afficher une boîte de dialogue de confirmation  
            var result = new ConfirmDeleteWindow();
            // Récupérer le job sélectionné dans le DataGrid  
            var selectedJob = jobsDataGrid.SelectedItem as Model.IBackupJobConfiguration;
            if (selectedJob != null)
            {
                // Supprimer le job de la liste  
                Model.Configuration.Instance.Jobs.Remove(selectedJob);
                jobsDataGrid.Items.Refresh();
            }
            else
            {
                System.Windows.MessageBox.Show("Veuillez sélectionner un job à supprimer.");
            }
        }

        // Démarrer le job sélectionné
        private void RunSelectedJobs_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Récupérer le job sélectionné dans le DataGrid  
            var selectedJob = jobsDataGrid.SelectedItem as Model.IBackupJobConfiguration;
            if (selectedJob != null)
            {
                // Cast explicit du DataContext pour accéder à la méthode StartJob  
                if (this.DataContext is IViewModel viewModel)
                {
                    // Créer une liste contenant le nom du job sélectionné  
                    var jobNameList = new List<string> { selectedJob.Name };

                    // Appeler la méthode avec la liste des noms  
                    viewModel.RunCommandRun(jobNameList);
                }
                else
                {
                    System.Windows.MessageBox.Show("Le DataContext n'est pas valide.");
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Veuillez sélectionner un job à démarrer.");
            }
        }
    }
}
