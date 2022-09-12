using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcpServer.VM
{
    public class MainWindowViewModel : Base
    {

        public MainWindowViewModel()
        {
            _server = new Server();
        }

        private string _isServerStart;
        public string IsServerStart
        {
            get { return _isServerStart; }
            set { SetProperty(ref _isServerStart, value); }
        }

        public DelegateCommand StartServerCommand { get { return new DelegateCommand(StartCommand); } }

        private async void StartCommand()
        {
            IsServerStart = "Start";
            await Server.ServerListenStart();
            IsServerStart = "End";
        }
        public DelegateCommand ServerEndCommand { get { return new DelegateCommand(ServerEnd); } }
        private  void ServerEnd()
        {
            Server.ServerStop();
        }


        

        private Server _server;
        public Server Server
        {
            get { return _server; }
        }


       
    }
}
