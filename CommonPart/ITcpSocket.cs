using System;
using System.Threading.Tasks;

namespace CommonPart
{
    public interface ITcpSocket
    {
        ITcpSocket RegisterDataCallback(Func<int, ReadOnlyMemory<byte>, ValueTask> callback);
        ITcpSocket RegisterSocketConnected(Func<int, ValueTask> callback);        
        ITcpSocket RegisterSocketDisconnected(Func<int, ValueTask> callback);
        ValueTask SendDataAsync(int socketId, ReadOnlyMemory<byte> buffer);
        ValueTask NotifyThatSocketConnected(int socketId);
        ValueTask NotifyThatSocketDisconnected(int socketId);
    }


    public static class ListenTcpSocketExts
    {
        
        public static ITcpSocket BindToSocket(this ITcpSocket from, ITcpSocket to)
        {
            from.RegisterDataCallback(to.SendDataAsync);    
            from.RegisterSocketConnected(to.NotifyThatSocketConnected);    
            from.RegisterSocketDisconnected(to.NotifyThatSocketDisconnected);

            return from;
        }
        
    }
    
}