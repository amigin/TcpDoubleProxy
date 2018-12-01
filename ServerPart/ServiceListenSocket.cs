using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using CommonPart;
using CommonPart.ServiceSocketHandler;

namespace ServerPart
{
    public class ServiceListenSocket : ITcpSocket, ISocketHandler, IConnectionsReader
    {
        private readonly TcpListener _listener;
        
        public ServiceListenSocket(int listenPort)
        {
            _listener = new TcpListener(IPAddress.Any, listenPort);
        }

        public Func<int, ReadOnlyMemory<byte>, ValueTask> DataCallback { get; private set; }
        /// <summary>
        /// Register callback with Parameters
        /// SocketId,
        /// Buffer,
        /// BufferDataSize
        /// </summary>
        /// <param name="callback"></param>
        public ITcpSocket RegisterDataCallback(Func<int, ReadOnlyMemory<byte>, ValueTask> callback)
        {
            DataCallback = callback;
            return this;
        }
        
        public async ValueTask GetDataFromSocketAsync(int socketId, ReadOnlyMemory<byte> data)
        {
            if (DataCallback != null)
                await DataCallback(socketId, data);
        }

        public Func<int, ValueTask> ConnectedCallback { get; private set; } 
        public ITcpSocket RegisterSocketConnected(Func<int, ValueTask> callback)
        {
            ConnectedCallback = callback;
            return this;
        }
        
        public async ValueTask Connected(int socketId)
        {
            if (ConnectedCallback != null)
                await ConnectedCallback(socketId);
            

        }


        public Func<int, ValueTask> DisconnectedCallback { get; private set; } 
        public ITcpSocket RegisterSocketDisconnected(Func<int, ValueTask> callback)
        {
            DisconnectedCallback = callback;
            return this;
        }
        
        public async ValueTask Disconnected(int socketId)
        {

            var socket = SocketsList.Remove(socketId);
            
            if (socket == null)
                return;
            
        //    Console.WriteLine($"S[{socketId}]: Disconnected service socket. Active Sockets: "+SocketsList.Count());
            
            if (DisconnectedCallback != null)
                await DisconnectedCallback(socketId);
        }
        

        private bool _working;

        public SocketsList<Tuple<ServiceSocketConnection, TcpClient>> SocketsList { get; } = new SocketsList<Tuple<ServiceSocketConnection, TcpClient>>();


        private int _currentSocketId;

        private void SocketConnected(TcpClient clientSocket)
        {
            _currentSocketId++;
            
            var socketId = _currentSocketId;
            var connection = new ServiceSocketConnection(clientSocket.GetStream(), this, socketId);

            SocketsList.Add(socketId, new Tuple<ServiceSocketConnection, TcpClient>(connection, clientSocket));
            connection.Start();
           // Console.WriteLine($"S[{socketId}]: Connected new service socket. Active Sockets: "+SocketsList.Count());

            
        }
        
        
        private async Task AcceptConnectionsThreadAsync()
        {
            while (_working)
            {
                var socket = await _listener.AcceptTcpClientAsync();
                SocketConnected(socket);
            }
        }
        
        public void Start()
        {
           _listener.Start();
           _working = true;
           Task.Run(AcceptConnectionsThreadAsync);
        }



        public async ValueTask SendDataAsync(int socketId, ReadOnlyMemory<byte> dataToSend)
        {
            var socket = SocketsList.Get(socketId);

            if (socket != null)
                await socket.Item1.SendDataAsync(dataToSend);
        }

        public async ValueTask NotifyThatSocketConnected(int socketId)
        {
            await Task.CompletedTask;
        }

        public async ValueTask NotifyThatSocketDisconnected(int socketId)
        {
            
            Console.WriteLine($"S[{socketId}]: Disconnecting service socket");
            var socket = SocketsList.Remove(socketId);

            if (socket != null)
                try
                {
                    socket.Item2.Close();
                }
                catch (Exception e)
                {
                 //   Console.WriteLine($"S[{socketId}]: Can not close tcp service connection. Reason: {e.Message}");

                }

            await Task.CompletedTask;
        }

        public IConnectionInfo[] GetConnections()
        {
            return SocketsList.GetAll().Select(itm => itm.Item1).Cast<IConnectionInfo>().ToArray();
        }
    }
}