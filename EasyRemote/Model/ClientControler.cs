using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EasyRemote
{
    public interface IClientControler
    {
        List<IBackupJob> Jobs { get; }
        List<IBackupJobState> JobsState { get; }
        public void ConnectToServer(Socket socket);
        public void DisconnectToServer(Socket socket);
        public void ListenToServer(Socket client);
        public void ListProcess();
        public void RunningProcess();
        public void RunProcess(String Name);
        public void PlayProcess(String Name);
        public void PauseProcess(String Name);
        public void CancelProcess(String Name);
        public void ResumeProcess(String Name);
    }
    class ClientControler
    {

    }
}
