using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EasyRemote.Model
{
    public interface IClientControler : INotifyPropertyChanged
    
    {
        public ObservableCollection<IBackupJob> BackupJob { get;}
        public ObservableCollection<IBackupJobState> RunningJobList { get; }
        public Socket ConfigureServer(string ipAddress, int port);
        public void ConnectToServer(Socket socket);
        public void DisconnectToServer(Socket socket);
        public void ListenToServer(Socket client);
        public void ListProcess();
        public void RunningProcess();
        public void RunProcess(String Name);
        public void PauseProcess(String Name);
        public void CancelProcess(String Name);
        public void ResumeProcess(String Name);
    }
    public class ClientController : IClientControler
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged( string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public ObservableCollection<IBackupJob> BackupJob { get; set; }
        public ObservableCollection<IBackupJobState> RunningJobList { get; }
        private static ClientController _instance;
        private static readonly object _lock = new object();

        public static ClientController Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new ClientController();
                        }
                    }
                }
                return _instance;
            }
        }

        private Socket _clientSocket;
        private Thread _listenerThread;
        private bool _isListening = false;

        private IPEndPoint _serverEndPoint;

        public string ServerIP { get; private set; }
        public int ServerPort { get; private set; }
        public bool IsConfigured => _serverEndPoint != null;
        public bool IsConnected => _clientSocket?.Connected ?? false;

        
        public event Action<List<IBackupJob>> JobsUpdated;
        public event Action<List<IBackupJobState>> JobsStateUpdated;
        public event Action<string> ConnectionStatusChanged;

        
        private ClientController()
        {
            BackupJob = new ObservableCollection<IBackupJob>();
        }
        public Socket ConfigureServer(string ipAddress, int port)
        {
            try
            {
                ServerIP = ipAddress;
                ServerPort = port;
                _serverEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);

                if (IsConnected)
                {
                    DisconnectToServer(null);
                }

                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                ConnectionStatusChanged?.Invoke($"Configuré pour {ipAddress}:{port}");

                return _clientSocket = socket;
            }
            catch (Exception ex)
            {
                ConnectionStatusChanged?.Invoke($"Erreur de configuration : {ex.Message}");
                return null;  // On renvoie null pour signaler l'échec
            }
        }

        public void ConnectToServer(Socket socket = null) 
        {
            if (!IsConfigured)
            {
                ConnectionStatusChanged?.Invoke("Serveur non configuré");
                return;
            }

            try
            {
                _clientSocket.Connect(_serverEndPoint);
                Console.WriteLine($"Connecté au serveur {ServerIP}:{ServerPort}");
                ConnectionStatusChanged?.Invoke("Connecté");

                _isListening = true;
                _listenerThread = new Thread(() => ListenToServer(_clientSocket));
                _listenerThread.IsBackground = true; 
                _listenerThread.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur de connexion : {ex.Message}");
                ConnectionStatusChanged?.Invoke($"Erreur : {ex.Message}");
            }
        }

        public void DisconnectToServer(Socket socket)
        {
            try
            {
                _isListening = false;

                if (_clientSocket != null && _clientSocket.Connected)
                {
                    _clientSocket.Shutdown(SocketShutdown.Both);
                    _clientSocket.Close();
                }

                _listenerThread?.Join(1000); 
                Console.WriteLine("Déconnecté.");
                ConnectionStatusChanged?.Invoke("Déconnecté");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la déconnexion : {ex.Message}");
            }
        }

        public void ListenToServer(Socket client)
        {
            try
            {
                while (_isListening && client.Connected)
                {
                    byte[] buffer = new byte[4096];
                    int received = client.Receive(buffer);
                    if (received == 0) break;

                    string message = Encoding.UTF8.GetString(buffer, 0, received);
                    HandleMessage(message);
                }
            }
            catch (SocketException)
            {
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la lecture des données : {ex.Message}");
                ConnectionStatusChanged?.Invoke("Connexion perdue");
            }
        }

        
        private void SendCommand(string command)
        {
            try
            {
                if (_clientSocket != null && _clientSocket.Connected)
                {
                    byte[] data = Encoding.UTF8.GetBytes(command);
                    _clientSocket.Send(data);
                }
                else
                {
                    Console.WriteLine("Pas de connexion au serveur.");
                    ConnectionStatusChanged?.Invoke("Pas de connexion");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de l'envoi de la commande : {ex.Message}");
            }
        }

       
        public void ListProcess()
        {
            SendCommand("list-json");
        }

        public void RunningProcess()
        {
            SendCommand("running");
        }

        public void RunProcess(string name)
        {
            SendCommand($"run|{name}");
        }

        public void PauseProcess(string name)
        {
            SendCommand($"pause|{name}");
        }

        public void CancelProcess(string name)
        {
            SendCommand($"cancel|{name}");
        }

        public void ResumeProcess(string name)
        {
            SendCommand($"resume|{name}");
        }

        private void HandleMessage(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (message.StartsWith("JOBS|"))
                {
                    var jobs = message.Substring(5).Split(',');
                    BackupJob.Clear();

                    foreach (var job in jobs.Where(j => !string.IsNullOrEmpty(j)))
                    {
                        var parts = job.Split(';');
                        if (parts.Length == 3)
                        {
                            string name = parts[0];
                            string source = parts[1];
                            string destination = parts[2];
                            bool isPaused = bool.Parse(parts[3]);

                            BackupJob.Add(new BackupJob
                            {
                                Name = name,
                                Source = source,
                                Destination = destination,
                                IsPaused = isPaused
                            });
                        }
                    }
                }
                else if (message.StartsWith("STATE|"))
                {
                    var states = message.Substring(6).Split(',');
                    RunningJobList.Clear();

                    foreach (var state in states.Where(s => !string.IsNullOrEmpty(s)))
                    {
                        var parts = state.Split(';');
                        if (parts.Length == 7)
                        {
                            string name = parts[0];
                            string sourcePath = parts[1];
                            string destinationPath = parts[2];

                            if (Enum.TryParse<State>(parts[3], out State jobState) &&
                                double.TryParse(parts[4], out double totalFiles) &&
                                double.TryParse(parts[5], out double totalSize) &&
                                int.TryParse(parts[6], out int progress))
                            {
                                var backupJob = BackupJob.FirstOrDefault(j => j.Name == name);

                                RunningJobList.Add(new BackupJobState
                                {
                                    BackupJob = backupJob,
                                    SourceFilePath = sourcePath,
                                    DestinationFilePath = destinationPath,
                                    State = jobState,
                                    TotalFilesToCopy = totalFiles,
                                    TotalFileSize = totalSize,
                                    Progression = progress
                                });
                            }
                        }
                    }
                }
            });
        }

        
       
    }
}

