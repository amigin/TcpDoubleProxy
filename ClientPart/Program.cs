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
            
            var techSocket = new TechnicalClientSocket("127.0.0.1", 5666);

            var serviceSocket = new ServiceClientSocket("192.168.1.50", 3389);


            techSocket.BindToSocket(serviceSocket);
            serviceSocket.BindToSocket(techSocket);
            
            techSocket.Start();
            
            Console.WriteLine("Started tech Socket...");
            while (true)
            {
                ConsoleUi.DrawUi(serviceSocket, techSocket);
                Thread.Sleep(3000);
            }
        }
    }
}