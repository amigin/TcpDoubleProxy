using System;

namespace CommonPart.TechSocket
{
    public static class TechSocketUtils
    {

        public static byte[] CreateSendDataTechSocketModel(this ReadOnlyMemory<byte> buffer, int socketId)
        {
            var result = new byte[buffer.Length+9];
            result[0] = SocketCommands.CommandGetData;
            result.WriteInt(socketId,1);
            result.WriteInt( buffer.Length,5);

            var bufferAsArray = buffer.ToArray();
            
            Array.Copy(bufferAsArray, 0, result, 9, bufferAsArray.Length);

            return result;
        }


        public static byte[] CreateSocketIsConnectedModel(this int socketId)
        {
            var bufferToSend = new byte[5];
            bufferToSend[0] = SocketCommands.CommandConnected;
            bufferToSend.WriteInt(socketId,1);
            return bufferToSend;
        }

        public static byte[] CreateSocketIsDisconnectedModel(this int socketId)
        {
            var bufferToSend = new byte[5];
            bufferToSend[0] = SocketCommands.CommandDisconnected;
            bufferToSend.WriteInt(socketId,1);
            return bufferToSend;
        }
        
        
    }
}