using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using CommonPart;
using CommonPart.TechSocket;

namespace ClientPart.TechnicalSocket
{
    public class TechnicalClientSocket : ITcpSocket, ISocketHandler, IConnectionsReader
    {
        private readonly string _ip;
        private readonly int _port;

        public TechnicalClientSocket(string ip, int port)
        {
            _ip = ip;
            _port = port;
        }

        private Tuple<TechSocketConnection, TcpClient> _currentConnection;



        private bool _working;



        private async Task PingThreadAsync(Tuple<TechSocketConnection, TcpClient> connection)
        {

            const int pingTimeout = 15;

            while (true)
            {

                await connection.Item1.SendPingAsync();

                if ((DateTime.UtcNow - connection.Item1.LastReceivedDataDateTime).TotalSeconds > pingTimeout)
                {
                    connection.Item2.Close();
                    throw new Exception("No income packets for more then {PingTimeout} seconds. Disconnect");
                }

                await Task.Delay(5000);

            }
            
        }
        

        
        private async Task SocketThreadAsync()
        {
            while (_working)
            {
                try
                {
                    var clientSocket = new TcpClient();

                    await clientSocket.ConnectAsync(_ip, _port);
                 //   Console.WriteLine($"Connected to tech socket {_ip}:{_port}");
                    var connection = new TechSocketConnection(clientSocket.GetStream(), this, 0, null);
                    
                    _currentConnection = new Tuple<TechSocketConnection, TcpClient>(connection, clientSocket);
            
                    connection.Start();

                    await PingThreadAsync(_currentConnection);

                }
                catch (Exception e)
                {
                  //  Console.WriteLine($"Can not connect to tech socket {_ip}:{_port}... " + e.Message);
                }
                finally
                {
                    _currentConnection = null;
                  //  Console.WriteLine("Next attempt in 3 seconds...");
                    await Task.Delay(3000);
                }
                
            }
        }


        public void Start()
        {
            if (_working)
                return;
            
            _working = true;
            Task.Run(SocketThreadAsync);
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

        private Func<int, ValueTask> _connectedCallback;

        public ITcpSocket RegisterSocketConnected(Func<int, ValueTask> callback)
        {
            _connectedCallback = callback;
            return this;
        }
        
        public async ValueTask Connected(int socketId)
        {
            if (_connectedCallback != null)
                await _connectedCallback(socketId);
        }


        private Func<int, ValueTask> _disconnectedCallback;
        
        public ITcpSocket RegisterSocketDisconnected(Func<int, ValueTask> callback)
        {
            _disconnectedCallback = callback;
            return this;
        }
        
        public async ValueTask Disconnected(int socketId)
        {
            if (_disconnectedCallback != null)
                await _disconnectedCallback(socketId);
        }




        public async ValueTask SendDataAsync(int socketId, ReadOnlyMemory<byte> buffer)
        {
            

            var socket = _currentConnection;
            if (socket == null)
                return;

            await socket.Item1.SendDataAsync(socketId, buffer);
        }

        public async ValueTask NotifyThatSocketConnected(int socketId)
        {
            var socket = _currentConnection;
            if (socket == null)
                return;
            
            await socket.Item1.SendClientConnectedAsync(socketId);
        }

        public async ValueTask NotifyThatSocketDisconnected(int socketId)
        {
            var socket = _currentConnection;
            if (socket == null)
                return;
            
            await socket.Item1.SendClientDisconnectedAsync(socketId);

        }


        public IConnectionInfo[] GetConnections()
        {
            var connection = _currentConnection;
            return connection == null ? Array.Empty<IConnectionInfo>() : new IConnectionInfo[] {connection.Item1};
        }
    }
}