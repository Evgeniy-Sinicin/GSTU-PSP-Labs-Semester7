using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LAB_3_SOLAE_TCP_SERVER_CS
{
    public class ThreadInfo
    {
        private bool _isWorking = false;

        public bool IsWorking { get => _isWorking; }
        public int Index { get; }
        public string Name { get; }
        public Socket Client { get; }

        public ThreadInfo(int index, string name, Socket client)
        {
            _isWorking = true;
            Index = index;
            Name = name;
            Client = client;
        }

        public void CompleteWork()
        {
            if (_isWorking)
            {
                _isWorking = false;
                Client.Shutdown(SocketShutdown.Both);
                Client.Close();
            }
        }
    }
}
