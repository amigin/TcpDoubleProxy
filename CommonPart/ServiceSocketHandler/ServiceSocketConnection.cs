using System;
using System.IO;
using System.Threading.Tasks;

namespace CommonPart.ServiceSocketHandler
{

    public class ServiceSocketConnection : IConnectionInfo
    {

        private readonly Stream _stream;

        private readonly ISocketHandler _socketHandler;
        public  int SocketId { get;}

        private bool _connected = true;
        private readonly byte[] _buffer = new byte[65535];

        public int ReceivedBytes { get; private set; }
        public int SentBytes { get; private set; }        
        
        public ServiceSocketConnection(Stream stream, ISocketHandler socketHandler, int socketId)
        {
            _stream = stream;
            _socketHandler = socketHandler;
            SocketId = socketId;
            socketHandler.Connected(socketId);
        }

        public void Start()
        {
            Task.Run(HandlerReadThreadAsync);
        }

        public async ValueTask HandleServiceSocketReadAsync()
        {
            var readLength = await _stream.ReadAsync(_buffer, 0, _buffer.Length);

            if (readLength == 0)
                throw new Exception("Disconnected");

            ReceivedBytes += readLength;

            var readOnlyMemory = new ReadOnlyMemory<byte>(_buffer, 0, readLength);

            await _socketHandler.GetDataFromSocketAsync(SocketId, readOnlyMemory);

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
              //  Console.WriteLine($"S[{SocketId}]: Service socket is disconnected. Reason:{e.Message}");
            }
            finally
            {
                await DisconnectAsync();
            }
        }

        private async ValueTask NotifySocketDisconnected()
        {
            try
            {
                await _socketHandler.Disconnected(SocketId);
            }
            catch (Exception e)
            {
              //  Console.WriteLine($"S[{SocketId}]: Error while handling disconnect callback for ListenSocket; Message: " + e.Message);
            }
        }


        public async ValueTask DisconnectAsync()
        {
            

            if (!_connected)
                return;

            _connected = false;

            await NotifySocketDisconnected();

        }


        public async ValueTask SendDataAsync(ReadOnlyMemory<byte> dataToSend)
        {
            try
            {
                await _stream.WriteAsync(dataToSend);
                SentBytes += dataToSend.Length;
            }
            catch (Exception e)
            {
                await NotifySocketDisconnected();
               // Console.WriteLine($"S[{SocketId}]: Could not send data to the socket; Reason: " + e.Message);
            }
        }


    }
    
}