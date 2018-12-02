using System;
using System.Threading;
using ClientPart.ServiceSocket;
using ClientPart.TechnicalSocket;
using CommonPart;

namespace ClientPart
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Please specify Service ip:port and Tech ip:port. Ex: dotnet run 192.168.1.5:5666 127.0.0.1:5656 ");
                return;
            }

            var srvSocketIp = args[0].Split(':')[0];
            var srvSocketPort = int.Parse(args[0].Split(':')[1]);

            var techSocketIp = args[1].Split(':')[0];
            var techSocketPort = int.Parse(args[1].Split(':')[1]);

            var techSocket = new TechnicalClientSocket(techSocketIp, techSocketPort);
            var serviceSocket = new ServiceClientSocket(srvSocketIp, srvSocketPort);

            techSocket.BindToSocket(serviceSocket);
            serviceSocket.BindToSocket(techSocket);

            techSocket.Start();

            Console.WriteLine("Started tech Socket...");
            while (true)
            {
                ConsoleUi.DrawUi(serviceSocket, techSocket);
                Console.WriteLine();
                Console.WriteLine("Press any key to refresh...");
                Console.ReadKey();
            }
        }
    }
}