using System;
using System.IO;
using System.Threading.Tasks;

namespace CommonPart
{
    public interface ISocketHandler
    {
        ValueTask Connected(int socketId);
        ValueTask Disconnected(int socketId);
        ValueTask GetDataFromSocketAsync(int socketId, ReadOnlyMemory<byte> data);
        
    }

  
}