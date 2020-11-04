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
    class Program
    {
        static void Main(string[] args)
        {
            var maxThreadsCount = Environment.ProcessorCount * 2;
            var minThreadsCount = 2;
            ThreadPool.SetMaxThreads(maxThreadsCount, maxThreadsCount);
            ThreadPool.SetMinThreads(minThreadsCount, minThreadsCount);

            var ip = Dns.GetHostAddresses(Dns.GetHostName())[Dns.GetHostAddresses(Dns.GetHostName()).Length - 1];
            var port = 27015;

            Console.WriteLine($"Server starts by address: {ip}:{port}");

            var server = new Server(ip, port);
        }
    }
}
