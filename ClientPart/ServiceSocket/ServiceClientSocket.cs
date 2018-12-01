using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using CommonPart;
using CommonPart.ServiceSocketHandler;

namespace ClientPart.ServiceSocket
{
    public class ServiceClientSocket : ITcpSocket, ISocketHandler, IConnectionsReader
    {
        private readonly string _destIp;
        private readonly int _destPort;


        public SocketsList<Tuple<ServiceSocketConnection, TcpClient>> SocketsList { get; } = new SocketsList<Tuple<ServiceSocketConnection, TcpClient>>();

        public ServiceClientSocket(string destIp, int destPort)
        {
            _destIp = destIp;
            _destPort = destPort;
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
                        
       //     Console.WriteLine($"S[{socketId}] Disconnecting. Active Service Sockets: "+SocketsList.Count());
            try
            {
                SocketsList.Remove(socketId);
                await DisconnectedCallback(socketId);
            }
            catch (Exception e)
            {
           //     Console.WriteLine($"S[{socketId}] Error while handling disconnect; Message: "+e.Message);
            }
            finally
            {
        //        Console.WriteLine($"S[{socketId}] Disconnected socket. Active Sockets: "+SocketsList.Count());
            }
        }

        public async ValueTask GetDataFromSocketAsync(int socketId, ReadOnlyMemory<byte> data)
        {
       //     Console.WriteLine($"S[{socketId}]: Received data from service. Len: "+data.Length);
            if (DataCallback != null)
                await DataCallback(socketId, data);
        }

        public async ValueTask SendDataAsync(int socketId, ReadOnlyMemory<byte> buffer)
        {
       //     Console.WriteLine($"S[{socketId}]: Sending data to service. Len:"+buffer.Length);
            var socket = SocketsList.Get(socketId);
            if (socket == null)
                return;

            await socket.Item1.SendDataAsync(buffer);

        }


        private async Task RunNewSocket(int socketId)
        {
          //  Console.WriteLine($"S[{socketId}]: New service connection...");
            try
            {
                var socket = new TcpClient();
                await socket.ConnectAsync(_destIp, _destPort);
                
                var connection = new ServiceSocketConnection(socket.GetStream(), this, socketId);
                SocketsList.Add(socketId, new Tuple<ServiceSocketConnection, TcpClient>(connection, socket));
           //     Console.WriteLine($"S[{socketId}]: Connected. Active Service Sockets: " + SocketsList.Count());
                connection.Start();
            }
            catch (Exception)
            {
              //  Console.WriteLine($"S[{socketId}]: Can not connect to the service... Send Disconnect back...");
                if (DisconnectedCallback != null)
                    await DisconnectedCallback(socketId);
            }

        }
        
        public async ValueTask NotifyThatSocketConnected(int socketId)
        {           
            await RunNewSocket(socketId);
        }

        public async ValueTask NotifyThatSocketDisconnected(int socketId)
        {
            var socket = SocketsList.Remove(socketId);

            if (socket != null)
                try
                {
                    socket.Item2.Close();
                }
                catch (Exception e)
                {
                //    Console.WriteLine($"S[{socketId}]: Can not close tcp service connection. Reason: {e.Message}");

                }

            await Task.CompletedTask;
            
        }


        public IConnectionInfo[] GetConnections()
        {
            return SocketsList.GetAll().Select(itm => itm.Item1).Cast<IConnectionInfo>().ToArray();
        }
    }
}