using System;
using System.IO;
using System.Threading.Tasks;

namespace CommonPart.TechSocket
{
    public class TechSocketConnection : IConnectionInfo
    {
        private readonly Stream _stream;

        private readonly ISocketHandler _socketHandler;
        private readonly Action<int> _techSocketDisconnected;

        private bool _connected = true;

        public Task TheTask { get; private set; }

        public int SocketId { get; }
        public int ReceivedBytes { get; private set; }
        public int SentBytes { get; private set; }
        
        public DateTime LastReceivedDataDateTime { get; private set; }

        public TechSocketConnection(Stream stream, ISocketHandler socketHandler, int techSocketId, Action<int> techSocketDisconnected)
        {
            _stream = stream;
            _socketHandler = socketHandler;
            _techSocketDisconnected = techSocketDisconnected;
            SocketId = techSocketId;
        }
        
        public void Start()
        {
            LastReceivedDataDateTime = DateTime.UtcNow;
            TheTask = HandlerReadThreadAsync();
        }
         
        private async ValueTask HandleSocketConnectedCommandAsync()
        {
            var socketId = await _stream.ReadIntFromSocketAsync();
            try
            {
               await _socketHandler.Connected(socketId);
            }
            catch (Exception e)
            {
              //  Console.WriteLine("Error Handling Tech Socket Connected. "+e.Message);
            }
        }
        
        private async ValueTask HandleSocketDisconnectedCommandAsync()
        {
            var socketId = await _stream.ReadIntFromSocketAsync();
            try
            {
               await _socketHandler.Disconnected(socketId);
            }
            catch (Exception e)
            {
           //     Console.WriteLine("Error Handling Tech Socket Connected. "+e.Message);
            }
        }
        
        private async ValueTask HandleSocketGetDataCommandAsync()
        {
            var socketId = await _stream.ReadIntFromSocketAsync();
            var dataLength = await _stream.ReadIntFromSocketAsync();
            var data = await _stream.ReadFromSocketAsync(dataLength);

            ReceivedBytes += dataLength;
            
            try
            {
                await _socketHandler.GetDataFromSocketAsync(socketId, data);
            }
            catch (Exception e)
            {
              //  Console.WriteLine("Error Handling Socket Data under tech socket. "+e.Message);
            }
        }



        private static readonly byte[] PongResponse = {SocketCommands.CommandPong};

        public async ValueTask HandleServiceSocketReadAsync()
        {
            var command = await _stream.ReadFromSocketAsync(1);   
            LastReceivedDataDateTime = DateTime.UtcNow;


            switch (command[0])
            {
                    
                case SocketCommands.CommandConnected:
                    await HandleSocketConnectedCommandAsync();
                    break;
                        
                case SocketCommands.CommandDisconnected:
                    await HandleSocketDisconnectedCommandAsync();
                    break;
                        
                case SocketCommands.CommandGetData:
                    await HandleSocketGetDataCommandAsync();
                    break;
                    
                case SocketCommands.CommandPing:
                    await _stream.WriteAsync(PongResponse);
                    break;
                    
            } 

        }

        public async Task HandlerReadThreadAsync()
        {

            try
            {
                while (_connected)
                    await HandleServiceSocketReadAsync();
            }
            catch (Exception e)
            {
              //  Console.WriteLine("Tech socket is disconnected. Reason: "+e.Message);
            }
            finally
            {
                _techSocketDisconnected?.Invoke(SocketId);
            }
        }




        private static readonly byte[] PingPacket = {SocketCommands.CommandPing};


        public async ValueTask SendPingAsync()
        {
            await _stream.WriteAsync(PingPacket);
        }



        public async ValueTask SendToSocket(ReadOnlyMemory<byte> dataToSend)
        {
            try
            {
                await _stream.WriteAsync(dataToSend);
            }
            catch (Exception e)
            {
              //  Console.WriteLine("Could not send data to the tech socket; Reason: " + e.Message);
            }
        }


        public async ValueTask SendDataAsync(int socketId, ReadOnlyMemory<byte> dataToSend)
        {
            var bufferToSend = dataToSend.CreateSendDataTechSocketModel(socketId);
            SentBytes += bufferToSend.Length;

            await SendToSocket(bufferToSend);
        }
        
        public async ValueTask SendClientConnectedAsync(int socketId)
        {
            var bufferToSend = socketId.CreateSocketIsConnectedModel();
            await SendToSocket(bufferToSend);
        }

        public async ValueTask SendClientDisconnectedAsync(int socketId)
        {
            var bufferToSend = socketId.CreateSocketIsDisconnectedModel();
            await SendToSocket(bufferToSend);
        }
        
    }
}