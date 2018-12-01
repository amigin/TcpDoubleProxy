using System;

namespace CommonPart
{

    public interface IConnectionsReader
    {

        IConnectionInfo[] GetConnections();

    }
    
    
    public interface IConnectionInfo{
        int SocketId { get; }
        int ReceivedBytes { get;  }
        int SentBytes { get; }  
    }
    
    public static class ConsoleUi
    {


        public static void DrawUi(IConnectionsReader serviceSocket, IConnectionsReader techSocket)
        {
            Console.Clear();

            
            var techConnections = techSocket.GetConnections();
            
            Console.WriteLine("Tech Connections: "+techConnections.Length);
            foreach (var connection in techConnections)
            {
                Console.WriteLine("Id: "+connection.SocketId+" Sent:"+connection.SentBytes+" Recv:"+connection.ReceivedBytes);
            }

            
            var connections = serviceSocket.GetConnections();
            
            Console.WriteLine("Service Connections: "+connections.Length);
            foreach (var connection in connections)
            {
                Console.WriteLine("Id: "+connection.SocketId+" Sent:"+connection.SentBytes+" Recv:"+connection.ReceivedBytes);
            }
           
           


        }
        
    }
}