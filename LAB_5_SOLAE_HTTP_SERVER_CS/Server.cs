using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LAB_5_SOLAE_HTTP_SERVER_CS
{
    class Server
    {
        private TcpListener _listener;

        public int Port { get; }
        public IPAddress Ip { get; }

        public Server(IPAddress ip, int port)
        {
            Port = port;
            Ip = ip;

            _listener = new TcpListener(Ip, port);
            _listener.Start();

            while (true)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(HandleClient), _listener.AcceptTcpClient());
            }
        }

        private void HandleClient(object parameters)
        {
            new Client((TcpClient)parameters);
        }

        ~Server()
        {
            if (_listener != null)
            {
                _listener.Stop();
            }
        }
    }
}
