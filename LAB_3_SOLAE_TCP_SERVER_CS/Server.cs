using System;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using MathNet.Numerics.LinearAlgebra.Double;
using LAB_3_SOLAE_TCP_CLIENT_CS;
using MathNet.Numerics.LinearAlgebra;

namespace LAB_3_SOLAE_TCP_SERVER_CS
{
    public static class Server
    {
        public static bool IsClientsReady { get; set; } = false;
        public static int Port { get; } = 27015;
        public static string Name { get; } = "Server";
        public static IPAddress Ip { get; set; } = Dns.GetHostAddresses(Dns.GetHostName())[Dns.GetHostAddresses(Dns.GetHostName()).Length - 1];

        public static void Main(string[] args)
        {
            Say("I start...");

            Say($"IP: {Ip}");
            Say($"Port: {Port}");

            Say(Name, "Enter required clients count: ", false);
            var requiredClientsCount = int.Parse(Console.ReadLine());
            var infos = new List<ThreadInfo>(requiredClientsCount);
            var isNotFinishedWork = true;
            var address = new IPEndPoint(Ip, Port);
            var server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                Say("I bind...");
                server.Bind(address);

                Say("I listen...");
                server.Listen((int)SocketOptionName.MaxConnections);

                Say("I handle of clients...");

                for (int i = 0; i < requiredClientsCount; i++)
                {
                    infos.Add(new ThreadInfo(i, $"Client Handler #{i}", server.Accept()));

                    if (!ThreadPool.QueueUserWorkItem(new WaitCallback(HandleClient), infos[i]))
                    {
                        Say($"Error! I couldn't create thread #{i} :(");
                    }

                    if (i == requiredClientsCount - 1)
                    {
                        IsClientsReady = true;
                    }
                }

                Say("I sleep...");
                do
                {
                    var isNeedBreak = false;

                    for (int i = 0; i < infos.Count && !isNeedBreak; i++)
                    {
                        if (infos[i].IsWorking)
                        {
                            isNeedBreak = !isNeedBreak;
                        }
                    }

                    if (!isNeedBreak)
                    {
                        isNotFinishedWork = !isNotFinishedWork;
                    }
                }
                while (isNotFinishedWork);
            }
            catch (Exception ex)
            {
                Say($"Error! {ex.Message}.");
            }
            finally
            {
                server.Close();
            }

            Say("I finish...");
            Say("Press any key to quit.");
            Console.ReadKey();

            return;
        }

        public static void HandleClient(object state)
        {
            var bytes = 0;
            var bufferSize = 1024;
            var buffer = new byte[bufferSize];
            ThreadInfo info = (ThreadInfo)state;

            Say(info.Name, "I start...");

            Say(info.Name, "I wait others...");
            while (!IsClientsReady);

            try 
            {
                Say(info.Name, $"I send index #{info.Index}...");
                buffer = Encoding.Unicode.GetBytes(info.Index.ToString());
                bytes = info.Client.Send(buffer);
                if (bytes <= 0)
                {
                    throw new Exception($"Unable to send index #{info.Index}");
                }

                Say(info.Name, "I receive work...");
                buffer = new byte[bufferSize];
                bytes = 0;
                bytes = info.Client.Receive(buffer, bufferSize, SocketFlags.None);
                if (bytes <= 0)
                {
                    throw new Exception($"Unable to receive work from client #{info.Index}");
                }
                Message message;
                using (var ms = new MemoryStream(buffer))
                {
                    message = (Message)((new BinaryFormatter()).Deserialize(ms));
                }

                Say(info.Name, "I send decision...");
                bytes = 0;
                Matrix<double> system = DenseMatrix.OfArray(message.System);
                Matrix<double> coeffs = DenseMatrix.OfArray(message.Coeffs);
                message.Decision = new Solae(system, coeffs).Solve().ToArray();
                using (var ms = new MemoryStream())
                {
                    new BinaryFormatter().Serialize(ms, message);
                    buffer = ms.ToArray();
                }
                bytes = info.Client.Send(buffer, buffer.Length, SocketFlags.None);
                if (bytes <= 0)
                {
                    throw new Exception($"Unable to send work index #{info.Index}");
                }
            }
            catch(Exception ex)
            {
                Say($"Error! {ex.Message}.");
            }
            
            info.CompleteWork();
            Say(info.Name, "I finish...");

            return;
        }

        private static void Say(string text)
        {
            Console.WriteLine($"{Name}: {text}");
        }

        private static void Say(string speakerName, string text, bool isNeedEnter = true)
        {
            Console.Write($"{speakerName}: {text}");

            if (isNeedEnter)
            {
                Console.WriteLine();
            }
        }
    }
}
