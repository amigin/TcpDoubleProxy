using System;
using System.Threading;
using CommonPart;
using ServerPart.TechnicalServerSocket;

namespace ServerPart
{
    class Program
    {

        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Please specify Service Port and Tech Port. Ex: dotnet run 3389 5666");
                return;
            }

            var clientListenPort = int.Parse(args[0]);

            var techListenPort = int.Parse(args[1]);

            var serviceListenSocket = new ServiceListenSocket(clientListenPort);
            var technicalListenSocket = new TechnicalListenSocket(techListenPort);

            serviceListenSocket.BindToSocket(technicalListenSocket);
            technicalListenSocket.BindToSocket(serviceListenSocket);

            technicalListenSocket.Start();
            serviceListenSocket.Start();

            Console.WriteLine("Client Socket started: " + clientListenPort);
            Console.WriteLine("Tech Socket started: " + techListenPort);

            while (true)
            {
                ConsoleUi.DrawUi(serviceListenSocket, technicalListenSocket);
                Console.WriteLine();
                Console.WriteLine("Press any key to refresh...");
                Console.ReadKey();
            }

        }

    }
}