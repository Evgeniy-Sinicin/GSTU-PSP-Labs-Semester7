using System;
using System.Collections.Generic;
using System.Threading;

namespace LAB_3_SOLAE_TCP_SERVER_CS
{
    public class Server
    {
        public static string Name { get; } = "Server";

        public static void Main(string[] args)
        {
            Say("I start...");

            Say(Name, "Enter required clients count: ", false);
            var requiredClientsCount = int.Parse(Console.ReadLine());
            var infos = new List<ThreadInfo>(requiredClientsCount);
            var isNotFinishedWork = true;

            Say("I handle of clients...");

            for (int i = 0; i < requiredClientsCount; i++)
            {
                infos.Add(new ThreadInfo($"Client Handler #{i}"));

                if (!ThreadPool.QueueUserWorkItem(new WaitCallback(HandleClient), infos[i]))
                {
                    Say($"Error! I couldn't create thread #{i} :(");
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

            Say("I finish...");
            Say("Press any key to quit.");
            Console.ReadKey();

            return;
        }

        public static void HandleClient(object state)
        {
            ThreadInfo info = (ThreadInfo)state;

            Say(info.Name, "I start...");

            Say(info.Name, "I do work...");
            
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
