using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CommonPart;
using CommonPart.TechSocket;

namespace TestsProject
{
    public class TechSocketMock : ISocketHandler, ITcpSocket
    {
        
        public MemoryStream Stream = new MemoryStream();

        public readonly TechSocketConnection TechSocketConnection;
        
        
        public List<string> RegisteredEvents = new List<string>();

        public TechSocketMock()
        {
            TechSocketConnection = new TechSocketConnection(Stream, this);   
        }



        public MemoryStream OtherStream;
        public void RegisterOtherStream(TechSocketMock otherMock)
        {
            OtherStream = otherMock.Stream;
        }


        public void PushDataViaSocket()
        {
            if (Stream.Length == 0)
                throw new Exception("No data at the Tech Socket");

            var bytes = Stream.ToArray();
            
            Stream.SetLength(0);
            OtherStream.SetLength(0);
            OtherStream.WriteAsync(bytes);
            OtherStream.Position = 0;
        }
        
        public async ValueTask Connected(int socketId)
        {
            RegisteredEvents.Add("Connected: "+socketId);
            await Task.CompletedTask;
        }

        public async ValueTask Disconnected(int socketId)
        {
            RegisteredEvents.Add("Disconnected: "+socketId);
            await Task.CompletedTask;
        }

        public async ValueTask GetDataFromSocketAsync(int socketId, ReadOnlyMemory<byte> data)
        {
            RegisteredEvents.Add("HasData: "+socketId+"; Data: "+data.ToHex());
            await DataCallback(socketId, data);
        }

        public Func<int, ReadOnlyMemory<byte>, ValueTask> DataCallback { get; private set; }
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

        
        public Func<int, ValueTask> DisconnectedCallback { get; private set; }        
        public ITcpSocket RegisterSocketDisconnected(Func<int, ValueTask> callback)
        {
            DisconnectedCallback = callback;
            return this;
        }

        public async ValueTask SendDataAsync(int socketId, ReadOnlyMemory<byte> buffer)
        {
            await TechSocketConnection.SendDataAsync(socketId, buffer);
        }

        public async ValueTask NotifyThatSocketConnected(int socketId)
        {
            await TechSocketConnection.SendClientConnectedAsync(socketId);
        }

        public async ValueTask NotifyThatSocketDisconnected(int socketId)
        {
            await TechSocketConnection.SendClientDisconnectedAsync(socketId);
        }
    }
}