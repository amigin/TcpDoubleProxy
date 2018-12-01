using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using CommonPart;
using CommonPart.TechSocket;

namespace ServerPart.TechnicalServerSocket
{
    public class TechnicalListenSocket : ITcpSocket, ISocketHandler, IConnectionsReader
    {

        private readonly TcpListener _listener;
        
        public TechnicalListenSocket(int listenPort)
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
          //  Console.WriteLine($"S[{socketId}]: Received data from service socket. Len: "+data.Length);

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
            
           // Console.WriteLine($"S[{socketId}]Got disconnect command for the socket...");
            if (DisconnectedCallback != null)
                await DisconnectedCallback(socketId);
        }

        public async ValueTask SendDataAsync(int socketId, ReadOnlyMemory<byte> buffer)
        {
            
          //  Console.WriteLine($"S[{socketId}]: Sending data to service. Len:"+buffer.Length);
            var sockets = _socketList.GetAll();

            foreach (var socket in sockets)
                await socket.Item1.SendDataAsync(socketId, buffer);
        }

        public async ValueTask NotifyThatSocketConnected(int socketId)
        {
            var sockets = _socketList.GetAll();

            foreach (var socket in sockets)
                await socket.Item1.SendClientConnectedAsync(socketId);
        }

        public async ValueTask NotifyThatSocketDisconnected(int socketId)
        {
            var sockets = _socketList.GetAll();

            foreach (var socket in sockets)
                await socket.Item1.SendClientDisconnectedAsync(socketId);
        }

        private bool _working;

        private readonly SocketsList<Tuple<TechSocketConnection, TcpClient>> _socketList 
            = new SocketsList<Tuple<TechSocketConnection, TcpClient>>();


        private int _currentSocketId;

        private void SocketConnected(TcpClient clientSocket)
        {
            _currentSocketId++;
            
            var socketId = _currentSocketId;

            var connection = new TechSocketConnection(clientSocket.GetStream(), this, _currentSocketId, TechSocketDisconnectedAsync);

            _socketList.Add(socketId, new Tuple<TechSocketConnection, TcpClient>(connection, clientSocket));
            
            connection.Start();
            
            
           // Console.WriteLine($"T[{socketId}]: Accepted tech socket. Active Sockets: "+_socketList.Count());
        }



        private void TechSocketDisconnectedAsync(int socketId)
        {
            var socket = _socketList.Remove(socketId);
            
            if (socket == null)
                return;


            try
            {
                socket.Item2.Close();
            }
            catch (Exception)
            {
//                Console.WriteLine(e);
            }

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


        public IConnectionInfo[] GetConnections()
        {
            return _socketList.GetAll().Select(itm => itm.Item1).Cast<IConnectionInfo>().ToArray();
        }
    }
}