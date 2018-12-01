﻿using System;
using System.Threading;
using CommonPart;
using ServerPart.TechnicalServerSocket;

namespace ServerPart
{
    class Program
    {
        
        static void Main(string[] args)
        {
            const int clientListenPort = 3389;
            const int techListenPort = 5666;
            
            var serviceListenSocket = new ServiceListenSocket(clientListenPort);
            var technicalListenSocket = new TechnicalListenSocket(techListenPort);

            serviceListenSocket.BindToSocket(technicalListenSocket);            
            technicalListenSocket.BindToSocket(serviceListenSocket);
            
            technicalListenSocket.Start();
            serviceListenSocket.Start();
            
            Console.WriteLine("Client Socket started: "+clientListenPort);
            Console.WriteLine("Tech Socket started: "+techListenPort);

            while (true)
            {
                ConsoleUi.DrawUi(serviceListenSocket, technicalListenSocket);
                Thread.Sleep(3000);
            }

        }

    }
}