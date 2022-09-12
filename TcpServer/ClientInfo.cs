using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TcpServer
{

    public class ClientInfo : Base
    {
        TcpClient _tcpClient;
        public ClientInfo(TcpClient client)
        {
            _tcpClient = client;
            _isSelected = false;
        }

         public TcpClient Client
        {
            get { return _tcpClient; }
        }
        public string IP
        {
            get { return _tcpClient.Client.RemoteEndPoint.ToString(); }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }
    }
}
