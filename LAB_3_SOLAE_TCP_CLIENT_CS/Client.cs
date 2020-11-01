using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using MathNet.Numerics.LinearAlgebra.Double;

namespace LAB_3_SOLAE_TCP_CLIENT_CS
{
    public static class Client
    {
        public static int Port { get; } = 27015;
        public static string Name { get; private set; } = "Client";
        public static IPAddress ServerIp { get; set; }

        public static void Main(string[] args)
        {
            var clientsCount = 4;
            var index = 0;
            var buffer = new byte[1024];
            var bytes = 0;
            var solaes = new List<Solae>(clientsCount)
            {
                new Solae(DenseMatrix.OfArray(new double[,] { { 2, 3, 1 },
                                                              { -2, 1, 0 },
                                                              { 1, 2, -2 }}), DenseMatrix.OfArray(new double[,] { { 3 },
                                                                                                                  { -2 },
                                                                                                                  { -1 }})),
                new Solae(DenseMatrix.OfArray(new double[,] { { -1, 3, 0 },
                                                              { 3, -2, 1 },
                                                              { 2, 1, -1 }}), DenseMatrix.OfArray(new double[,] { { 4 },
                                                                                                                  { -3 },
                                                                                                                  { -3 }})),
                new Solae(DenseMatrix.OfArray(new double[,] { { -2, -1, 6 },
                                                              { 1, -1, 2 },
                                                              { 2, 4, -3 }}), DenseMatrix.OfArray(new double[,] { { 31 },
                                                                                                                  { 13 },
                                                                                                                  { 10 }})),
                new Solae(DenseMatrix.OfArray(new double[,] { { 3, -2, 4 },
                                                              { 3, 4, -2 },
                                                              { 2, -1, -1 }}), DenseMatrix.OfArray(new double[,] { { 21 },
                                                                                                                   { 9 },
                                                                                                                   { 10 }}))
            };

            Say("I start...");

            try
            {
                Say(Name, "Enter server ip: ", false);
                ServerIp = IPAddress.Parse(Console.ReadLine());
                var address = new IPEndPoint(ServerIp, Port);
                var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                Say("I connect...");
                client.Connect(address);

                Say("I receive index...");
                bytes = client.Receive(buffer);
                if (bytes <= 0)
                {
                    throw new Exception("Unable to receive index");
                }
                index = int.Parse(Encoding.Unicode.GetString(buffer));
                Say($"My index: {index}");
                Name += $" #{index}";
                buffer = null;

                Say("I send work...");
                var message = new Message(solaes[index].System, solaes[index].Coeffs);
                using (var ms = new MemoryStream())
                {
                    new BinaryFormatter().Serialize(ms, message);
                    buffer = ms.ToArray();
                }
                bytes = client.Send(buffer, buffer.Length, SocketFlags.None);
                if (bytes <= 0)
                {
                    throw new Exception("Unable to send work");
                }

                Say("I receive and print decision...");
                bytes = 0;
                var bufferSize = 1024;
                buffer = new byte[bufferSize];
                bytes = client.Receive(buffer, bufferSize, SocketFlags.None);
                if (bytes <= 0)
                {
                    throw new Exception($"Unable to receive work from client #{index}");
                }
                using (var ms = new MemoryStream(buffer))
                {
                    message = (Message)((new BinaryFormatter()).Deserialize(ms));
                }

                Say("I print result...");
                var precision = 5;
                PrintMatrix(message.System, $"System #{index}", precision);
                PrintMatrix(message.Coeffs, $"Coeffs #{index}", precision);
                PrintMatrix(message.Decision, $"Decision #{index}", precision);

                Say("I close...");
                client.Shutdown(SocketShutdown.Both);
                client.Close();
            }
            catch(Exception ex)
            {
                Say($"Error! {ex.Message}.");
            }

            Say("I finish...");
            Say("Press any key to quit.");
            Console.ReadKey();

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

        private static void PrintMatrix(double[,] matrix, string label, int precision)
        {
            var padding = 1;
            var dm = DenseMatrix.OfArray(matrix);

            for (int i = 0; i < padding; i++)
            {
                Console.Write("=== ");
            }

            Console.Write($"{label} ");

            for (int i = 0; i < padding; i++)
            {
                Console.Write("=== ");
            }

            Console.WriteLine();

            for (int i = 0; i < dm.RowCount; i++)
            {
                for (int j = 0; j < padding; j++)
                {
                    Console.Write("\t");
                }

                for (int j = 0; j < dm.ColumnCount; j++)
                {
                    Console.Write($"{Math.Round(dm[i, j], precision)} ");
                }

                Console.WriteLine();
            }

            for (int i = 0; i < padding * 5; i++)
            {
                Console.Write("=== ");
            }

            Console.WriteLine();
        }
    }
}
