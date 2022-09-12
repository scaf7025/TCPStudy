using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Threading;
using System.Collections.ObjectModel;

namespace TcpServer
{
    public class Server : Base
    {
        public class DataReceivedEventArgs : EventArgs
        {
            public TcpClient Client { get; private set; }
            public DataReceivedEventArgs(TcpClient client)
            {
                Client = client;
            }
        }

        TcpListener _server;
        CancellationTokenSource _tokenSource;
        private bool _serverListening;
        private CancellationToken _token;
        public bool ServerListening => _serverListening;
        public event EventHandler<DataReceivedEventArgs> OnDataReceived;
        public Progress<TcpClient> _prograssReport = new Progress<TcpClient>();

        private List<ClientInfo> _clientList = new List<ClientInfo>();
        public List<ClientInfo> Clients
        {
            get { return _clientList; }
            set { SetProperty(ref _clientList, value); }
        }

        public Server()
        {
            _server = new TcpListener(IPAddress.Loopback, 7000);
            OnDataReceived += Server_OnDataReceived;
            
        }
        public Server(IPAddress address, int port)
        {
            _server = new TcpListener(address, port);
            OnDataReceived += Server_OnDataReceived;
        }

        private async void Server_OnDataReceived(object sender, DataReceivedEventArgs e)
        {
            var client = e.Client;
            var endPoint = client.Client.RemoteEndPoint.ToString();
            try
            {
                
                var ns = e.Client.GetStream();
                using StreamWriter sw = new StreamWriter(ns) { AutoFlush = true };
                using StreamReader sr = new StreamReader(ns);

                while (true)
                {
                    var rec = await sr.ReadLineAsync();
                    Console.WriteLine($"{DateTime.Now}-{rec}");

                    //비동기 처리 테스트
                    _ =Task.Run(async () => {
                        await Task.Delay(10000);
                        sw.WriteLine($"{Thread.CurrentThread.ManagedThreadId} - SometingDo {DateTime.Now}");
                        Console.WriteLine($"{DateTime.Now}");
                        sw.WriteLine($"{Thread.CurrentThread.ManagedThreadId} - SometingDo End {DateTime.Now}");

                        //echo
                        foreach (var item in _clientList)
                        {
                            if (item.Client.Connected == true)
                            {
                                StreamWriter d = new StreamWriter(item.Client.GetStream()) { AutoFlush = true };
                                d.WriteLine("echo"+rec);
                            }

                        }
                    });
                }
            }
            catch (Exception)
            {
                var toRemove = _clientList.Where(c => c.Client.Client.Connected ==false);
                _clientList.Clear();
                _clientList.AddRange(toRemove);
                Console.WriteLine($"{endPoint} removed");
                Console.WriteLine($"Current Client = {_clientList.Count}");
            }



            //var bytesRead = 0;
            //do
            //{
            //    bytesRead = e.Stream.Read(new byte[1024], 0, 1024);
            //}
            //while (bytesRead > 0 && e.Stream.DataAvailable);

            //// Simulate long running task
            //Console.WriteLine($"Doing some heavy response processing now. [{Thread.CurrentThread.ManagedThreadId}]");
            //await Task.Delay(3000);
            //Console.WriteLine($"Finished processing. [{Thread.CurrentThread.ManagedThreadId}]");

            //var response = Encoding.ASCII.GetBytes("Who's there?");
            //e.Stream.Write(response, 0, response.Length);
        }

        public async Task ServerListenStart(CancellationToken? token = null)
        {

            _tokenSource = CancellationTokenSource.CreateLinkedTokenSource(token ?? new CancellationToken());
            _token = _tokenSource.Token;
            _server.Start();
            _serverListening = true;
            _serverStartTime = DateTime.Now;
            Console.WriteLine($"StartServer-{DateTime.Now}");
            try
            {
                while (!_token.IsCancellationRequested)
                {
                    await Task.Run(async () =>
                    {
                        var tcpClientTask = _server.AcceptTcpClientAsync();
                        var result = await tcpClientTask;

                        Console.WriteLine(string.Format($"{result.Client.RemoteEndPoint} - Connected"));
                        _clientList.Add(new ClientInfo(result));
                        Console.WriteLine(string.Format($"Current Client Count {_clientList.Count}"));

                        OnDataReceived?.Invoke(this, new DataReceivedEventArgs(result));
                    }, _token);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                _server.Stop();
                Console.WriteLine($"ServerEnd-{DateTime.Now}");
            }
        }

        public void ServerStop()
        {
            Console.WriteLine($"Server Stop request{DateTime.Now}");
            if (_tokenSource != null)
            {
                _tokenSource.Cancel();
            }
            _server.Stop();
        }


        public async Task SendMsgClient(TcpClient client, string msg)
        {
            if (client.Connected == true)
            {
                StreamWriter streamWriter = new StreamWriter(client.GetStream());
                await streamWriter.WriteLineAsync("From Server"+msg);
            }
        }


        //public async Task HandleClientConnection(TcpClient client)
        //{
        //     NetworkStream ns = client.GetStream();
        //     StreamWriter sw = new StreamWriter(ns);
        //     StreamReader sr = new StreamReader(ns);

        //    Progress<string> _prograssReport = new Progress<string>();
        //    _prograssReport.ProgressChanged += _prograssReport_ProgressChanged1;    
        //    var pro = _prograssReport as IProgress<string>;
        //    pro.Report(string.Format($"{DateTime.Now} : {client.Client.RemoteEndPoint} Client Connected"));
        //    string returnMsg = string.Format($"{DateTime.Now} : Server Connected");
        //    await sw.WriteLineAsync(returnMsg);


        //       string msg = await sr.ReadLineAsync();
        //        if (!String.IsNullOrEmpty(msg))
        //        {
        //            pro.Report(string.Format($"{DateTime.Now} : {client.Client.RemoteEndPoint} -{msg}"));

        //        }
        //        if (msg == "End")
        //        {
        //        return;
        //        }

        //    Console.WriteLine("DDDDD");
        //    pro.Report(string.Format($"{DateTime.Now} : {client.Client.RemoteEndPoint} Client Connection END"));
        //    await ns.FlushAsync();
        //}




        private DateTime _serverStartTime;






    }
}
